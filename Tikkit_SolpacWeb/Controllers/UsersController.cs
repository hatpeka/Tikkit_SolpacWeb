using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Tikkit_SolpacWeb.Data;
using Tikkit_SolpacWeb.Models;
using Tikkit_SolpacWeb.Services.Email;

namespace Tikkit_SolpacWeb.Controllers
{
    public class UsersController : Controller
    {
        private readonly Tikkit_SolpacWebContext _context;
        private readonly EmailSender _emailSender;


        public UsersController(Tikkit_SolpacWebContext context, EmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        private Users GetCurrentUser()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
                return null;

            var currentUser = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            return currentUser;
        }

        public IActionResult UserAction()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            if (currentUser.Role == "Admin")
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Edit", new { id = currentUser.ID });
            }
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var User = _context.Users.FirstOrDefault(u => u.Email == email);
            if (User != null && User.Email == email && User.Password == password && User.Status == "Working")
            {
                HttpContext.Session.SetInt32("UserId", User.ID);
                HttpContext.Session.SetString("UserEmail", User.Email);
                HttpContext.Session.SetString("UserRole", User.Role);
                HttpContext.Session.SetString("UserName", User.Name);
                HttpContext.Session.SetString("Partner", User.Partner);

                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return RedirectToAction("Index", "Requests");
                
            }
            return RedirectToAction("Login");
        }


        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return View("ForgotPasswordConfirmation");
                }

                string emailSubject = $"Mật khẩu ứng dụng";
                string emailMessage = $"Chào {user.Name}, mật khẩu của bạn là {user.Password}";
                await _emailSender.SendEmailAsync(email, emailSubject, emailMessage);
            }

            return RedirectToAction("Login");
        }


        // GET: Users
        [RequireLogin]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();

            var partners = await _context.Partners.ToListAsync();

            foreach (var user in users)
            {
                var partner = partners.FirstOrDefault(p => p.PartnerID == user.PartnerID);
            }

            return View(users);
        }


        // GET: Users/Create
        public IActionResult Create()
        {
            var partners = _context.Partners.ToList();
            ViewBag.Partners = partners;

            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,PartnerID,Partner,Address,Sex,Phone,Email,Password,RePassword,Role,Status")] Users users)
        {

            var partners = _context.Partners.ToList();
            ViewBag.Partners = partners;

            if (ModelState.IsValid)
            {
                var partner = partners.FirstOrDefault(p => p.PartnerID == users.PartnerID);
                users.Partner = partner.Name;
                users.RePassword = null;
                _context.Add(users);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(users);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            var partners = _context.Partners.ToList();
            ViewBag.Partners = partners;

            string userRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = userRole;

            var currentUser = GetCurrentUser();
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            ViewData["CurrentUser"] = currentUser;

            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }
            return View(users);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,PartnerID,Address,Sex,Phone,Email,Role,Status")] Users users)
        {
            var partners = _context.Partners.ToList();
            ViewBag.Partners = partners;

            if (id != users.ID)
            {
                return NotFound();
            }

            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            var partner = partners.FirstOrDefault(p => p.PartnerID == users.PartnerID);

            ModelState.Remove("Password");
            ModelState.Remove("RePassword");
            if (ModelState.IsValid)
            {
                try
                {
                    existingUser.Name = users.Name;
                    existingUser.PartnerID = users.PartnerID;

                    // Cập nhật tên của partner dựa trên PartnerID
                    if (partner != null)
                    {
                        existingUser.Partner = partner.Name;
                    }

                    existingUser.Address = users.Address;
                    existingUser.Sex = users.Sex;
                    existingUser.Phone = users.Phone;
                    existingUser.Email = users.Email;
                    existingUser.Role = users.Role;
                    existingUser.Status = users.Status;

                    // Password and RePassword are not updated

                    await _context.SaveChangesAsync();
                    string? userName = HttpContext.Session.GetString("UserName");
                    ViewBag.UserName = userName;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersExists(users.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return NotFound();
                }
                if (currentUser.Role == "Admin")
                {
                    return RedirectToAction("Index", "Users");

                }
                return RedirectToAction("Index", "Requests");
            }
            return View(users);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .FirstOrDefaultAsync(m => m.ID == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'Tikkit_SolpacWebContext.Users'  is null.");
            }
            var users = await _context.Users.FindAsync(id);
            if (users != null)
            {
                _context.Users.Remove(users);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserRole");
            return RedirectToAction("Login");
        }

        private bool UsersExists(int id)
        {
          return (_context.Users?.Any(e => e.ID == id)).GetValueOrDefault();
        }

        // GET: Users/ChangePassword
        public IActionResult ChangePassword()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            return View(currentUser);
        }

        // POST: Users/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, [Bind("ID,CurrentPassword,NewPassword,ConfirmPassword")] Users users)
        {
            if (id != users.ID)
            {
                return NotFound();
            }

            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return NotFound();
            }

            // Verify the current password
            if (users.CurrentPassword != currentUser.Password)
            {
                ModelState.AddModelError("CurrentPassword", "The current password is incorrect.");
                return View(users);
            }
            if (users.NewPassword != users.ConfirmPassword)
            {
                return View(users);
            }
            // Save the new password as plain text
            currentUser.Password = users.NewPassword;
            _context.Update(currentUser);
            await _context.SaveChangesAsync();

            // Redirect to the main page of the role after saving changes.
            if (currentUser.Role == "Admin")
            {
                return RedirectToAction(nameof(Index));
            }
            else if (currentUser.Role == "Staff")
            {
                return RedirectToAction("Staff", "Home");
            }
            else if (currentUser.Role == "Client")
            {
                return RedirectToAction("Client", "Home");
            }

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> ExportUsersToExcel()
        {
            var users = _context.Users.AsQueryable();

            var usersList = await users.ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Tạo tiêu đề cho các cột
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Tên";
                worksheet.Cells[1, 3].Value = "Đối tác";
                worksheet.Cells[1, 4].Value = "Địa chỉ";
                worksheet.Cells[1, 5].Value = "Giới tính";
                worksheet.Cells[1, 6].Value = "Điện thoại";
                worksheet.Cells[1, 7].Value = "Email";
                worksheet.Cells[1, 8].Value = "Nhóm người dùng";

                // Đổ dữ liệu vào bảng
                for (int i = 0; i < usersList.Count; i++)
                {
                    var user = usersList[i];
                    worksheet.Cells[i + 2, 1].Value = i + 1;
                    worksheet.Cells[i + 2, 2].Value = user.Name;
                    worksheet.Cells[i + 2, 3].Value = user.Partner;
                    worksheet.Cells[i + 2, 4].Value = user.Address;
                    worksheet.Cells[i + 2, 5].Value = user.Sex;
                    worksheet.Cells[i + 2, 6].Value = user.Phone;
                    worksheet.Cells[i + 2, 7].Value = user.Email;
                    worksheet.Cells[i + 2, 8].Value = user.Role;
                }

                // Ghi file Excel vào MemoryStream
                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);

                // Trả file Excel về cho người dùng
                string fileName = "UsersReport.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                stream.Position = 0;
                return File(stream, contentType, fileName);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportUsersFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file selected");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        Users user = new Users
                        {
                            Name = worksheet.Cells[row, 2].Value.ToString().Trim(),
                            Partner = worksheet.Cells[row, 3].Value.ToString().Trim(),
                            Address = worksheet.Cells[row, 4].Value.ToString().Trim(),
                            Sex = worksheet.Cells[row, 5].Value.ToString().Trim(),
                            Phone = worksheet.Cells[row, 6].Value.ToString().Trim(),
                            Email = worksheet.Cells[row, 7].Value.ToString().Trim(),
                            Password = "1",
                            RePassword = "1",
                            Role = worksheet.Cells[row, 8].Value.ToString().Trim()
                        };

                        _context.Users.Add(user);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

    }
}
