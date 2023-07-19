﻿using System;
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
using Microsoft.Extensions.Hosting.Internal;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using Tikkit_SolpacWeb.Hubs;
using Microsoft.AspNetCore.Identity;
using System.Collections;
using System.Security.Claims;

namespace Tikkit_SolpacWeb.Controllers
{
    public class RequestsController : Controller
    {
        private readonly Tikkit_SolpacWebContext _context;
        private readonly EmailSender _emailSender;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHubContext<NotificationHub> _hubContext;
        public RequestsController(Tikkit_SolpacWebContext context, EmailSender emailSender, IWebHostEnvironment hostingEnvironment, IHubContext<NotificationHub> hubcontext)
        {
            _context = context;
            _emailSender = emailSender;
            _hostingEnvironment = hostingEnvironment;
            _hubContext = hubcontext;
        }

        // GET: Requests
        [RequireLogin]
        public async Task<IActionResult> Index(int? id, string search, DateTime? fromDate, DateTime? toDate, string partner, string priority, string createPerson, string project, string status, string supporter)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            string userName = HttpContext.Session.GetString("UserName");
            var requests = _context.Requests.AsQueryable();
            ViewBag.UserRole = userRole;

            if (id.HasValue)
            {
                requests = requests.Where(r => r.RequestNo == id.Value);
            }
            requests = requests.Where(r =>
                (userRole != "Staff" || (r.Status == "Đang chờ" || r.Supporter == userName)) &&
                (userRole == "Staff" || r.RequestPerson == userName || r.CreatePerson == userName) &&
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
            var userId = HttpContext.Session.GetInt32("UserId");
            var notifications = _context.Notification
                .Where(n => n.Target == userId) // Filter notifications based on the current userId
                .ToList();
            ViewBag.Notifications = notifications;

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
            var users = _context.Users.ToList();
            ViewBag.Users = users;

            var userId = HttpContext.Session.GetInt32("UserId");
            var notifications = _context.Notification
                .Where(n => n.Target == userId) // Filter notifications based on the current userId
                .ToList();
            ViewBag.Notifications = notifications;

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
        public async Task<IActionResult> Create([Bind("RequestNo,RequestDate,DeadlineDate,Partner,Project,RequestPersonID,SubjectOfRequest,ContentsOfRequest,ImagePath,Priority,Contact")] Requests requests, IFormFile? Image)
        {
            if (Image != null && Image.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Image.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                requests.ImagePath = "/uploads/" + uniqueFileName;
            }
            if (ModelState.IsValid)
            {
                var requestUser = _context.Users.SingleOrDefault(u => u.ID == requests.RequestPersonID);
                requests.CreatePerson = HttpContext.Session.GetString("UserName");
                requests.RequestPerson = requestUser.Name;
                requests.Partner = requestUser.Partner;
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

                string notificationMessage = $"Client {requests.RequestPerson} has created a new request.";

                var staffUsers = _context.Users
                    .Where(u => u.Role == "Staff")
                    .ToList();
                foreach (var staffUser in staffUsers)
                {
                    var notification = new Notification
                    {
                        Target = staffUser.ID,
                        CreateTime = DateTime.Now,
                        Title = $"Client {requests.RequestPerson} has a new request.",
                        RequestID = requests.RequestNo
                    };
                    _context.Add(notification);
                    await _context.SaveChangesAsync();

                    await _hubContext.Clients.User(staffUser.ID.ToString()).SendAsync("ReceiveNotification", notification.Title);
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

            var users = _context.Users.ToList();
            ViewBag.Users = users;
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

            var requestPerson = await _context.Users.FirstOrDefaultAsync(u => u.ID == existingRequest.RequestPersonID);
            string requestPersonEmail = requestPerson?.Email;


            if (ModelState.IsValid)
            {
                if (existingRequest.Status == "Đang chờ")
                {
                    existingRequest.Status = "Đang xử lý";
                    existingRequest.Supporter = HttpContext.Session.GetString("UserName");
                    existingRequest.StartDate = DateTime.Now;
                    existingRequest.ExpectedDate = request.ExpectedDate;
                    existingRequest.Reason = request.Reason;
                    existingRequest.SupportContent = request.SupportContent;

                    try
                    {
                        if (!string.IsNullOrEmpty(requestPersonEmail))
                        {
                            string subject = "Thông báo về yêu cầu";
                            string message = $"Yêu cầu của bạn đã được tiếp nhận bởi: {existingRequest.Supporter}.\n " +
                                $"Ngày dự kiến: {existingRequest.ExpectedDate}";
                            await _emailSender.SendEmailAsync(requestPersonEmail, subject, message);
                        }

                        var notification = new Notification
                        {
                            Target = existingRequest.RequestPersonID, // this should be the client's user ID
                            CreateTime = DateTime.Now,
                            Title = $"Your request has been received by staff {existingRequest.Supporter}.",
                            RequestID = existingRequest.RequestNo,
                        };
                        _context.Add(notification);
                        await _context.SaveChangesAsync();

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
                else if(existingRequest.Status == "Đang xử lý")
                {
                    existingRequest.Status = "Đã hoàn thành";
                    existingRequest.EndDate = DateTime.Now; 
                    existingRequest.Reason = request.Reason;
                    existingRequest.SupportContent = request.SupportContent;


                    try
                    {
                        if (!string.IsNullOrEmpty(requestPersonEmail))
                        {
                            string subject = "Thông báo về yêu cầu";
                            string message = $"Yêu cầu của bạn đã được hoàn thành bởi: {existingRequest.Supporter}. \n" +
                                $"Thời gian bắt đầu: {existingRequest.StartDate}\n" +
                                $"Thời gian kết thúc: {existingRequest.EndDate}\n" +
                                $"Nội dung hỗ trợ: {existingRequest.SupportContent} \n" +
                                $"Tổng thời gian đã hỗ trợ: {existingRequest.TotalTime} \n";
                            await _emailSender.SendEmailAsync(requestPersonEmail, subject, message);
                        }

                        var notification = new Notification
                        {
                            Target = existingRequest.RequestPersonID,
                            CreateTime = DateTime.Now,
                            Title = $"Your request has been done by staff {existingRequest.Supporter}.",
                            RequestID = existingRequest.RequestNo,
                        };
                        _context.Add(notification);
                        await _context.SaveChangesAsync();

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
                (userRole != "Staff" || (r.Status == "Pending" || r.Supporter == userName)) &&
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
                worksheet.Cells[1, 3].Value = "Ngày bắt đầu";
                worksheet.Cells[1, 4].Value = "Ngày dự kiến";
                worksheet.Cells[1, 5].Value = "Thời gian bắt đầu";
                worksheet.Cells[1, 6].Value = "Thời gian kết thúc";
                worksheet.Cells[1, 7].Value = "Ngày kết thúc";
                worksheet.Cells[1, 8].Value = "Mức ưu tiên";
                worksheet.Cells[1, 9].Value = "Trạng thái";
                worksheet.Cells[1, 10].Value = "Người yêu cầu";
                worksheet.Cells[1, 11].Value = "Người tạo yêu cầu";
                worksheet.Cells[1, 12].Value = "Người hỗ trợ";
                worksheet.Cells[1, 13].Value = "Công ty";
                worksheet.Cells[1, 14].Value = "Dự án";
                worksheet.Cells[1, 15].Value = "Tiêu đề yêu cầu";
                worksheet.Cells[1, 16].Value = "Nội dung yêu cầu";
                worksheet.Cells[1, 17].Value = "Hình ảnh";
                worksheet.Cells[1, 18].Value = "Nguyên nhân";
                worksheet.Cells[1, 19].Value = "Nội dung hỗ trợ";
                worksheet.Cells[1, 20].Value = "Tổng thời gian";

                // Đổ dữ liệu vào bảng
                for (int i = 0; i < requestsList.Count; i++)
                {
                    var request = requestsList[i];
                    worksheet.Cells[i + 2, 1].Value = i + 1;
                    worksheet.Cells[i + 2, 2].Value = request.RequestDate.ToString("dd-MM");
                    worksheet.Cells[i + 2, 3].Value = request.StartDate.HasValue ? request.StartDate.Value.ToString("dd-MM") : "";
                    worksheet.Cells[i + 2, 4].Value = request.ExpectedDate.HasValue ? request.ExpectedDate.Value.ToString("dd-MM") : "";
                    worksheet.Cells[i + 2, 5].Value = request.StartDate.HasValue ? request.StartDate.Value.ToString("hh:mm:ss") : "";
                    worksheet.Cells[i + 2, 6].Value = request.EndDate.HasValue ? request.EndDate.Value.ToString("hh:mm:ss") : "";
                    worksheet.Cells[i + 2, 7].Value = request.EndDate.HasValue ? request.EndDate.Value.ToString("dd-MM") : "";
                    worksheet.Cells[i + 2, 8].Value = request.Priority;
                    worksheet.Cells[i + 2, 9].Value = request.Status;
                    worksheet.Cells[i + 2, 10].Value = request.RequestPerson;
                    worksheet.Cells[i + 2, 11].Value = request.CreatePerson;
                    worksheet.Cells[i + 2, 12].Value = request.Supporter;
                    worksheet.Cells[i + 2, 13].Value = request.Partner;
                    worksheet.Cells[i + 2, 14].Value = request.Project;
                    worksheet.Cells[i + 2, 15].Value = request.SubjectOfRequest;
                    worksheet.Cells[i + 2, 16].Value = request.ContentsOfRequest;
                    //worksheet.Cells[i + 2, 17].Value = request.Image;
                    worksheet.Cells[i + 2, 18].Value = request.Reason;
                    worksheet.Cells[i + 2, 19].Value = request.SupportContent;
                    worksheet.Cells[i + 2, 20].Value = request.TotalTime;
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

        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a file.");
                return RedirectToAction("Index"); ; // Replace with the view you want to return
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("File", "Invalid file format. Please upload an Excel file (.xlsx).");
                return RedirectToAction("Index"); ; // Replace with the view you want to return
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var startDate = worksheet.Cells[row, 3].Value?.ToString();
                            var startTime = worksheet.Cells[row, 5].Value?.ToString();
                            DateTime? startDateValue = null;
                            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(startTime))
                            {
                                DateTime parsedDate;
                                if (DateTime.TryParseExact($"{startDate}{startTime}", "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                                {
                                    startDateValue = parsedDate;
                                }
                                else
                                {
                                    // Handle the case where the date could not be parsed
                                }
                            }

                            var endDate = worksheet.Cells[row, 7].Value?.ToString();
                            var endTime = worksheet.Cells[row, 6].Value?.ToString();
                            DateTime? endDateValue = null;
                            if (!string.IsNullOrEmpty(endDate) && !string.IsNullOrEmpty(endTime))
                            {
                                DateTime parsedDate;
                                if (DateTime.TryParseExact($"{endDate}{endTime}", "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                                {
                                    endDateValue = parsedDate;
                                }
                                else
                                {
                                    // Handle the case where the date could not be parsed
                                }
                            }

                            DateTime requestDate;
                            if (!DateTime.TryParseExact(worksheet.Cells[row, 2].Value.ToString(), "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out requestDate))
                            {
                                // Handle the case where the date could not be parsed
                            }

                            var expectedDateString = (worksheet.Cells[row, 4].Value ?? "").ToString();
                            DateTime? expectedDate = null;
                            if (!string.IsNullOrEmpty(expectedDateString))
                            {
                                DateTime parsedDate;
                                if (DateTime.TryParseExact(expectedDateString, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                                {
                                    expectedDate = parsedDate;
                                }
                                else
                                {
                                    // Handle the case where the date could not be parsed
                                }
                            }

                            var request = new Requests
                            {
                                RequestDate = requestDate,
                                StartDate = startDateValue,
                                ExpectedDate = expectedDate,
                                EndDate = endDateValue,
                                Priority = worksheet.Cells[row, 8].Value.ToString(),
                                RequestPerson = worksheet.Cells[row, 10].Value.ToString(),
                                CreatePerson = worksheet.Cells[row, 11].Value.ToString(),
                                Supporter = worksheet.Cells[row, 12]?.Value?.ToString(),
                                Partner = worksheet.Cells[row, 13].Value.ToString(),
                                Project = worksheet.Cells[row, 14].Value.ToString(),
                                SubjectOfRequest = worksheet.Cells[row, 15].Value.ToString(),
                                ContentsOfRequest = worksheet.Cells[row, 16].Value.ToString(),
                                ImagePath = "",
                                Reason = worksheet.Cells[row, 18]?.Value?.ToString(),
                                SupportContent = worksheet.Cells[row, 19]?.Value?.ToString(),
                            };
                            _context.Requests.Add(request);
                        };
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("File", "An error occurred while processing the file. Please make sure the file format is correct.");
                return RedirectToAction("Index"); // Replace with the view you want to return
            }
            return RedirectToAction("Staff", "Home"); // Replace with the action you want to redirect to after successful import
        }
        private bool RequestsExists(int id)
        {
          return (_context.Requests?.Any(e => e.RequestNo == id)).GetValueOrDefault();
        }
    }
}
