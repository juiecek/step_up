using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using step_up.Models;

namespace step_up.Controllers
{
    public class DanceStylesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanceStylesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DanceStyles
        public async Task<IActionResult> Index()
        {
            return View(await _context.DanceStyles.ToListAsync());
        }

        // GET: DanceStyles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danceStyle = await _context.DanceStyles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (danceStyle == null)
            {
                return NotFound();
            }

            return View(danceStyle);
        }

        // GET: DanceStyles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DanceStyles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] DanceStyle danceStyle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danceStyle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(danceStyle);
        }

        // GET: DanceStyles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danceStyle = await _context.DanceStyles.FindAsync(id);
            if (danceStyle == null)
            {
                return NotFound();
            }
            return View(danceStyle);
        }

        // POST: DanceStyles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] DanceStyle danceStyle)
        {
            if (id != danceStyle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danceStyle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanceStyleExists(danceStyle.Id))
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
            return View(danceStyle);
        }

        // GET: DanceStyles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danceStyle = await _context.DanceStyles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (danceStyle == null)
            {
                return NotFound();
            }

            return View(danceStyle);
        }

        // POST: DanceStyles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danceStyle = await _context.DanceStyles.FindAsync(id);
            if (danceStyle != null)
            {
                _context.DanceStyles.Remove(danceStyle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DanceStyleExists(int id)
        {
            return _context.DanceStyles.Any(e => e.Id == id);
        }
    }
}
