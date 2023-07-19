using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tikkit_SolpacWeb.Data;
using Tikkit_SolpacWeb.Models;

namespace Tikkit_SolpacWeb.Controllers
{
    public class UsersController : Controller
    {
        private readonly Tikkit_SolpacWebContext _context;

        public UsersController(Tikkit_SolpacWebContext context)
        {
            _context = context;
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


        // GET: Users
        [RequireLogin]
        public async Task<IActionResult> Index()
        {
              return _context.Users != null ? 
                          View(await _context.Users.ToListAsync()) :
                          Problem("Entity set 'Tikkit_SolpacWebContext.Users'  is null.");
        }


        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Partner,Address,Sex,Phone,Email,Password,RePassword,Role,Status")] Users users)
        {
            if (ModelState.IsValid)
            {
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
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Partner,Address,Sex,Phone,Email,Role,Status")] Users users)
        {
            if (id != users.ID)
            {
                return NotFound();
            }

            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            ModelState.Remove("Password");
            ModelState.Remove("RePassword");
            if (ModelState.IsValid)
            {
                try
                {
                    existingUser.Name = users.Name;
                    existingUser.Partner = users.Partner;
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
    }
}
