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

namespace Tikkit_SolpacWeb.Controllers
{
    public class RequestsController : Controller
    {
        private readonly Tikkit_SolpacWebContext _context;
        private readonly EmailSender _emailSender;

        public RequestsController(Tikkit_SolpacWebContext context, EmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
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
        public async Task<IActionResult> Create([Bind("RequestNo,RequestDate,DeadlineDate,Partner,Project,RequestPerson,SubjectOfRequest,ContentsOfRequest,Priority,Contact")] Requests requests)
        {
            string userRole = HttpContext.Session.GetString("Role");
            if (userRole == "Staff")
            {
                return Forbid(); // Deny access to staff users
            }

            if (ModelState.IsValid)
            {
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

        private bool RequestsExists(int id)
        {
          return (_context.Requests?.Any(e => e.RequestNo == id)).GetValueOrDefault();
        }
    }
}
