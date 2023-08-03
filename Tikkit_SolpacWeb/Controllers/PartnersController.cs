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
    public class PartnersController : Controller
    {
        private readonly Tikkit_SolpacWebContext _context;

        public PartnersController(Tikkit_SolpacWebContext context)
        {
            _context = context;
        }

        // GET: Partners
        public async Task<IActionResult> Index()
        {
              return _context.Partners != null ? 
                          View(await _context.Partners.ToListAsync()) :
                          Problem("Entity set 'Tikkit_SolpacWebContext.Partners'  is null.");
        }

        // GET: Partners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Partners == null)
            {
                return NotFound();
            }

            var partners = await _context.Partners
                .FirstOrDefaultAsync(m => m.PartnerID == id);
            if (partners == null)
            {
                return NotFound();
            }

            return View(partners);
        }

        // GET: Partners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Partners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] Partners partners)
        {
            if (ModelState.IsValid)
            {
                _context.Add(partners);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(partners);
        }

        // GET: Partners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Partners == null)
            {
                return NotFound();
            }

            var partners = await _context.Partners.FindAsync(id);
            if (partners == null)
            {
                return NotFound();
            }
            return View(partners);
        }

        // POST: Partners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name")] Partners partners)
        {
            if (id != partners.PartnerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(partners);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartnersExists(partners.PartnerID))
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
            return View(partners);
        }

        // GET: Partners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Partners == null)
            {
                return NotFound();
            }

            var partners = await _context.Partners
                .FirstOrDefaultAsync(m => m.PartnerID == id);
            if (partners == null)
            {
                return NotFound();
            }

            return View(partners);
        }

        // POST: Partners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Partners == null)
            {
                return Problem("Entity set 'Tikkit_SolpacWebContext.Partners'  is null.");
            }
            var partners = await _context.Partners.FindAsync(id);
            if (partners != null)
            {
                _context.Partners.Remove(partners);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PartnersExists(int id)
        {
          return (_context.Partners?.Any(e => e.PartnerID == id)).GetValueOrDefault();
        }
    }
}
