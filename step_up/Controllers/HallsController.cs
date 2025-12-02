using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using step_up.Models;

namespace step_up.Controllers
{
    public class HallsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HallsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Halls
        public async Task<IActionResult> Index()
        {
            return View(await _context.Hall.ToListAsync());
        }

        // GET: Halls/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Halls/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Halls hall, IFormFile PhotoUpload)
        {
            if (ModelState.IsValid)
            {
                // Обработка фотографии
                if (PhotoUpload != null && PhotoUpload.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(PhotoUpload.FileName).ToLower();

                    // Проверка расширения файла
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("Photo", "Недопустимый формат изображения. Допустимы только .jpg, .jpeg, .png, .gif.");
                        return View(hall);
                    }

                    // Генерация уникального имени для файла
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    // Сохранение файла
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await PhotoUpload.CopyToAsync(stream);
                    }

                    hall.Photo = "/images/" + fileName;
                }

                _context.Hall.Add(hall);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(hall);
        }

        // GET: Halls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Hall.FindAsync(id);
            if (hall == null) return NotFound();

            return View(hall);
        }

        // POST: Halls/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Halls hall, IFormFile PhotoUpload)
        {
            if (id != hall.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingHall = await _context.Hall.FindAsync(id);

                    if (existingHall == null) return NotFound();

                    existingHall.Name = hall.Name;
                    existingHall.Description = hall.Description;
                    existingHall.Capacity = hall.Capacity;

                    // Фото
                    if (PhotoUpload != null && PhotoUpload.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(PhotoUpload.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("Photo", "Недопустимый формат изображения. Допустимы только .jpg, .jpeg, .png, .gif.");
                            return View(hall);
                        }

                        var fileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await PhotoUpload.CopyToAsync(stream);
                        }

                        existingHall.Photo = "/images/" + fileName;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HallExists(hall.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(hall);
        }

        // GET: Halls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Hall.FirstOrDefaultAsync(m => m.Id == id);
            if (hall == null) return NotFound();

            return View(hall);
        }

        // POST: Halls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hall = await _context.Hall.FindAsync(id);

            if (hall != null)
            {
                _context.Hall.Remove(hall);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HallExists(int id)
        {
            return _context.Hall.Any(e => e.Id == id);
        }
    }
}
