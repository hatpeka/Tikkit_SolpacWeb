using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tikkit_SolpacWeb.Data;
using Tikkit_SolpacWeb.Models;

namespace Tikkit_SolpacWeb.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly Tikkit_SolpacWebContext _context;

        public ProjectsController(Tikkit_SolpacWebContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var tikkit_SolpacWebContext = _context.Projects.Include(p => p.Partners);
            return View(await tikkit_SolpacWebContext.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var projects = await _context.Projects
                .Include(p => p.Partners)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (projects == null)
            {
                return NotFound();
            }

            return View(projects);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            var partners = _context.Partners.ToList();
            ViewBag.Partners = partners;

            ViewData["PartnerID"] = new SelectList(_context.Partners, "PartnerID", "PartnerID");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,PartnerID")] Projects projects)
        {
            try
            {
                var partners = _context.Partners.ToList();
                ViewBag.Partners = partners;

                if (ModelState.IsValid)
                {
                    _context.Add(projects);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // In ra các lỗi trong ModelState
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Model state error: {error.ErrorMessage}");
                    }
                }

                ViewData["PartnerID"] = new SelectList(_context.Partners, "PartnerID", "PartnerID", projects.PartnerID);
            }
            catch (Exception ex)
            {
                // In thông báo lỗi ra màn hình Output
                System.Diagnostics.Debug.WriteLine($"Error creating new project: {ex.Message}");
                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            ViewData["PartnerID"] = new SelectList(_context.Partners, "PartnerID", "PartnerID", projects.PartnerID);
            return View(projects);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var partners = _context.Partners.ToList();
            ViewBag.Partners = partners;

            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var projects = await _context.Projects.FindAsync(id);
            if (projects == null)
            {
                return NotFound();
            }
            ViewData["PartnerID"] = new SelectList(_context.Partners, "PartnerID", "PartnerID", projects.PartnerID);
            return View(projects);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,PartnerID")] Projects projects)
        {
            var partners = _context.Partners.ToList();
            ViewBag.Partners = partners;

            if (id != projects.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projects);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectsExists(projects.ID))
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
            ViewData["PartnerID"] = new SelectList(_context.Partners, "PartnerID", "PartnerID", projects.PartnerID);
            return View(projects);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var projects = await _context.Projects
                .Include(p => p.Partners)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (projects == null)
            {
                return NotFound();
            }

            return View(projects);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'Tikkit_SolpacWebContext.Projects'  is null.");
            }
            var projects = await _context.Projects.FindAsync(id);
            if (projects != null)
            {
                _context.Projects.Remove(projects);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectsExists(int id)
        {
          return (_context.Projects?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
