using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Tikkit_SolpacWeb.Data;
using Tikkit_SolpacWeb.Models;
using Tikkit_SolpacWeb.Services.Email;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace Tikkit_SolpacWeb.Controllers
{
    public class RequestsController : Controller
    {
        private readonly Tikkit_SolpacWebContext _context;
        private readonly EmailSender _emailSender;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public RequestsController(Tikkit_SolpacWebContext context, EmailSender emailSender, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _emailSender = emailSender;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Requests
        [RequireLogin]
public async Task<IActionResult> Index(string search, DateTime? fromDate, DateTime? toDate, string partner, string priority, string createPerson, string project, string status, string supporter)
{
    string userRole = HttpContext.Session.GetString("UserRole");
    string userName = HttpContext.Session.GetString("UserName");
    var requests = _context.Requests.AsQueryable();
    ViewBag.UserRole = userRole;

    requests = requests.Where(r =>
        (userRole == "Staff" || r.CreatePerson == userName) &&
        (!fromDate.HasValue || r.RequestDate >= fromDate.Value) &&
        (!toDate.HasValue || r.RequestDate <= toDate.Value) &&
        (string.IsNullOrEmpty(partner) || r.Partner.Contains(partner)) && 
        (string.IsNullOrEmpty(priority) || r.Priority.Contains(priority)) &&
        (string.IsNullOrEmpty(createPerson) || r.CreatePerson.Contains(createPerson)) &&
        (string.IsNullOrEmpty(project) || r.Project.Contains(project)) &&
        (string.IsNullOrEmpty(status) || r.Status.Contains(status)) &&
        (string.IsNullOrEmpty(supporter) || r.Supporter.Contains(supporter)) &&
        (string.IsNullOrEmpty(search) || r.SubjectOfRequest.Contains(search) || r.ContentsOfRequest.Contains(search))
    );

    return View(await requests.ToListAsync());
}


        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Requests == null)
            {
                return NotFound();
            }

            var requests = await _context.Requests
                .FirstOrDefaultAsync(m => m.RequestNo == id);
            if (requests == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial",requests);
        }

        // GET: Requests/Create
        public IActionResult Create()
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Staff")
            {
                return Forbid(); // Deny access to staff users
            }

            return View();
        }

        // POST: Requests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestNo,RequestDate,DeadlineDate,Partner,Project,RequestPerson,SubjectOfRequest,ContentsOfRequest,ImagePath,Priority,Contact")] Requests requests, IFormFile ImagePath)
        {
            string userRole = HttpContext.Session.GetString("Role");
            if (userRole == "Staff")
            {
                return Forbid(); // Deny access to staff users
            }

            if (ModelState.IsValid)
            {
                if (ImagePath != null && ImagePath.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImagePath.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImagePath.CopyToAsync(fileStream);
                    }
                    requests.ImagePath = "/uploads/" + uniqueFileName;
                }
                requests.CreatePerson = HttpContext.Session.GetString("UserName");
                requests.Partner = HttpContext.Session.GetString("Partner");
                _context.Add(requests);
                await _context.SaveChangesAsync();

                var staffEmails = _context.Users
                    .Where(u => u.Role == "Staff")
                    .Select(u => u.Email)
                    .ToList();

                string emailSubject = $"New request: {requests.SubjectOfRequest}";
                string emailMessage = $"Client {requests.RequestPerson} has created a new request with the following details:\n\n{requests.ContentsOfRequest}";
                foreach (string staffEmail in staffEmails)
                {
                    await _emailSender.SendEmailAsync(staffEmail, emailSubject, emailMessage);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(requests);
        }

        // GET: Requests/Edit/5
        public async Task<IActionResult> EditforStaff(int? id)
        {
            if (id == null || _context.Requests == null)
            {
                return NotFound();
            }

            var requests = await _context.Requests.FindAsync(id);
            if (requests == null)
            {
                return NotFound();
            }
            return PartialView("_EditforStaffPartial",requests);
        }

        // POST: Requests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditforStaff(int id, [Bind("RequestNo,RequestDate,DeadlineDate,Partner,Project,RequestPerson,SubjectOfRequest,ContentsOfRequest,Priority,Contact")] Requests requests)
        {
            if (id != requests.RequestNo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(requests);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestsExists(requests.RequestNo))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(requests);
        }


        // GET: Requests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Requests == null)
            {
                return NotFound();
            }

            var requests = await _context.Requests
                .FirstOrDefaultAsync(m => m.RequestNo == id);
            if (requests == null)
            {
                return NotFound();
            }

            return View(requests);
        }

        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Requests == null)
            {
                return Problem("Entity set 'Tikkit_SolpacWebContext.Requests'  is null.");
            }
            var requests = await _context.Requests.FindAsync(id);
            if (requests != null)
            {
                _context.Requests.Remove(requests);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Requests/Response/5
        public async Task<IActionResult> StaffResponse(int? id)
        {
            if (id == null || _context.Requests == null)
            {
                return NotFound();
            }

            var request = await _context.Requests.FirstOrDefaultAsync(m => m.RequestNo == id);

            if (request == null)
            {
                return NotFound();
            }

            return PartialView("_StaffResponsePartial",request);
        }

        // POST: Requests/Response/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StaffResponse(int id, [Bind("RequestNo,StartDate,ExpectedDate,EndDate,Supporter,Reason,SupportContent")] Requests request)
        {
            if (id != request.RequestNo)
            {
                return NotFound();
            }

            if (_context.Requests == null)
            {
                return Problem("Entity set 'Tikkit_SolpacWebContext.Requests'  is null.");
            }

            var existingRequest = await _context.Requests.FirstOrDefaultAsync(m => m.RequestNo == id);
            if (existingRequest == null)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                if (existingRequest.Status == "Pending")
                {
                    existingRequest.Status = "Processing";
                    existingRequest.Supporter = HttpContext.Session.GetString("UserName");
                    existingRequest.StartDate = DateTime.Now;
                    existingRequest.ExpectedDate = request.ExpectedDate;
                    existingRequest.Reason = request.Reason;
                    existingRequest.SupportContent = request.SupportContent;

                    try
                    {
                        _context.Update(existingRequest);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!RequestsExists(existingRequest.RequestNo))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                else if(existingRequest.Status == "Processing")
                {
                    existingRequest.Status = "Done";
                    existingRequest.EndDate = DateTime.Now; 
                    existingRequest.Reason = request.Reason;
                    existingRequest.SupportContent = request.SupportContent;


                    try
                    {
                        _context.Update(existingRequest);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!RequestsExists(existingRequest.RequestNo))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string search, DateTime? fromDate, DateTime? toDate, string partner, string priority, string createPerson, string project, string status, string supporter)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            string userName = HttpContext.Session.GetString("UserName");
            var requests = _context.Requests.AsQueryable();

            requests = requests.Where(r =>
                (userRole == "Staff" || r.CreatePerson == userName) &&
                (!fromDate.HasValue || r.RequestDate >= fromDate.Value) &&
                (!toDate.HasValue || r.RequestDate <= toDate.Value) &&
                (string.IsNullOrEmpty(partner) || r.Partner.Contains(partner)) &&
                (string.IsNullOrEmpty(priority) || r.Priority.Contains(priority)) &&
                (string.IsNullOrEmpty(createPerson) || r.CreatePerson.Contains(createPerson)) &&
                (string.IsNullOrEmpty(project) || r.Project.Contains(project)) &&
                (string.IsNullOrEmpty(status) || r.Status.Contains(status)) &&
                (string.IsNullOrEmpty(supporter) || r.Supporter.Contains(supporter)) &&
                (string.IsNullOrEmpty(search) || r.SubjectOfRequest.Contains(search) || r.ContentsOfRequest.Contains(search))
            );

            var requestsList = await requests.ToListAsync();
            // Tạo file Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Requests");

                // Tạo tiêu đề cho các cột
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Ngày yêu cầu";
                worksheet.Cells[1, 3].Value = "Ngày dự kiến";
                worksheet.Cells[1, 4].Value = "Thời gian bắt đầu";
                worksheet.Cells[1, 5].Value = "Thời gian kết thúc";
                worksheet.Cells[1, 6].Value = "Ngày kết thúc";
                worksheet.Cells[1, 7].Value = "Mức ưu tiên";
                worksheet.Cells[1, 8].Value = "Trạng thái";
                worksheet.Cells[1, 9].Value = "Người yêu cầu";
                worksheet.Cells[1, 10].Value = "Người tạo yêu cầu";
                worksheet.Cells[1, 11].Value = "Người hỗ trợ";
                worksheet.Cells[1, 12].Value = "Công ty";
                worksheet.Cells[1, 13].Value = "Dự án";
                worksheet.Cells[1, 14].Value = "Tiêu đề yêu cầu";
                worksheet.Cells[1, 15].Value = "Nội dung yêu cầu";
                worksheet.Cells[1, 16].Value = "Hình ảnh";
                worksheet.Cells[1, 17].Value = "Nguyên nhân";
                worksheet.Cells[1, 18].Value = "Nội dung hỗ trợ";
                worksheet.Cells[1, 19].Value = "Tổng thời gian";

                // Đổ dữ liệu vào bảng
                for (int i = 0; i < requestsList.Count; i++)
                {
                    var request = requestsList[i];
                    worksheet.Cells[i + 2, 1].Value = i + 1;
                    worksheet.Cells[i + 2, 2].Value = request.RequestDate.ToString("dd-MM");
                    worksheet.Cells[i + 2, 3].Value = request.ExpectedDate.HasValue ? request.ExpectedDate.Value.ToString("dd-MM") : "";
                    worksheet.Cells[i + 2, 4].Value = request.StartDate.HasValue ? request.StartDate.Value.ToString("hh-mm") : "";
                    worksheet.Cells[i + 2, 4].Value = request.EndDate.HasValue ? request.EndDate.Value.ToString("hh-mm") : "";
                    worksheet.Cells[i + 2, 4].Value = request.EndDate.HasValue ? request.EndDate.Value.ToString("dd-MM") : "";
                    worksheet.Cells[i + 2, 7].Value = request.Priority;
                    worksheet.Cells[i + 2, 8].Value = request.Status;
                    worksheet.Cells[i + 2, 9].Value = request.RequestPerson;
                    worksheet.Cells[i + 2, 10].Value = request.CreatePerson;
                    worksheet.Cells[i + 2, 11].Value = request.Supporter;
                    worksheet.Cells[i + 2, 12].Value = request.Partner;
                    worksheet.Cells[i + 2, 13].Value = request.Project;
                    worksheet.Cells[i + 2, 14].Value = request.SubjectOfRequest;
                    worksheet.Cells[i + 2, 15].Value = request.ContentsOfRequest;
                    //worksheet.Cells[i + 2, 16].Value = request.Image;
                    worksheet.Cells[i + 2, 17].Value = request.Reason;
                    worksheet.Cells[i + 2, 18].Value = request.SupportContent;
                    worksheet.Cells[i + 2, 19].Value = request.TotalTime;
                }

                // Ghi file Excel vào MemoryStream
                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);

                // Trả file Excel về cho người dùng
                string fileName = "RequestsReport.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                stream.Position = 0;
                return File(stream, contentType, fileName);
            }
        }

        private bool RequestsExists(int id)
        {
          return (_context.Requests?.Any(e => e.RequestNo == id)).GetValueOrDefault();
        }
    }
}
