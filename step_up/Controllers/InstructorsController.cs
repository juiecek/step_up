using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using step_up.Models;
using step_up.Models.ViewModels;

namespace step_up.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InstructorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Instructors
        public async Task<IActionResult> Index()
        {
            var model = new InstructorsWithReviewsViewModel
            {
                Instructors = await _context.Instructor.ToListAsync(),
                Reviews = await _context.InstructorReviews
                    .Include(r => r.Instructor)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync()
            };

            return View(model);
        }

        // GET: Instructors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var instructor = await _context.Instructor
                .Include(i => i.DanceStyle) // Заменяем связь с DanceStyle
                .FirstOrDefaultAsync(m => m.Id == id);

            if (instructor == null) return NotFound();

            return View(instructor);
        }

        // GET: Instructors/Create
        public IActionResult Create()
        {
            // Отображаем список доступных стилей танцев
            ViewBag.DanceStyles = new SelectList(_context.DanceStyles, "Id", "Name");
            return View();
        }

        // POST: Instructors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instructors instructor, IFormFile PhotoUpload)
        {
            if (PhotoUpload != null && PhotoUpload.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(PhotoUpload.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Photo", "Недопустимый формат изображения.");
                }
                else
                {
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await PhotoUpload.CopyToAsync(stream);
                    }

                    instructor.Photo = "/images/" + fileName;
                }
            }
            else
            {
                ModelState.AddModelError("Photo", "Фото обязательно для загрузки.");
            }

            if (ModelState.IsValid)
            {
                _context.Instructor.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DanceStyles = new SelectList(_context.DanceStyles, "Id", "Name", instructor.DanceStyleId);
            return View(instructor);
        }



        // GET: Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var instructor = await _context.Instructor.FindAsync(id);
            if (instructor == null)
                return NotFound();

            ViewBag.DanceStyles = new SelectList(_context.DanceStyles, "Id", "Name", instructor.DanceStyleId);
            return View(instructor);
        }

        // POST: Instructors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Instructors instructor, IFormFile PhotoUpload)
        {
            if (id != instructor.Id)
                return NotFound();

            var existingInstructor = await _context.Instructor.FindAsync(id);
            if (existingInstructor == null)
                return NotFound();

            if (PhotoUpload != null && PhotoUpload.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(PhotoUpload.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Photo", "Недопустимый формат изображения.");
                }
                else
                {
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await PhotoUpload.CopyToAsync(stream);
                    }

                    existingInstructor.Photo = "/images/" + fileName;
                }
            }

            if (ModelState.IsValid)
            {
                existingInstructor.FullName = instructor.FullName;
                existingInstructor.Description = instructor.Description;
                existingInstructor.Phone = instructor.Phone;
                existingInstructor.DanceStyleId = instructor.DanceStyleId;

                _context.Update(existingInstructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DanceStyles = new SelectList(_context.DanceStyles, "Id", "Name", instructor.DanceStyleId);
            return View(instructor);
        }


        // GET: Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var instructor = await _context.Instructor
                .FirstOrDefaultAsync(m => m.Id == id);

            if (instructor == null) return NotFound();

            return View(instructor);
        }

        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await _context.Instructor
                .FirstOrDefaultAsync(m => m.Id == id);

            if (instructor != null)
            {
                _context.Instructor.Remove(instructor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InstructorsExists(int id)
        {
            return _context.Instructor.Any(e => e.Id == id);
        }
    }

}
