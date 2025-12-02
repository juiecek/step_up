using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using step_up.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace step_up.Controllers
{
    [Authorize]
    public class DanceStyleReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanceStyleReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DanceStyleReviews
        public async Task<IActionResult> Index()
        {
            var reviews = _context.DanceStyleReviews
                .Include(r => r.DanceStyle)
                .Include(r => r.Schedule)
                .Include(r => r.User);
            return View(await reviews.ToListAsync());
        }

        // GET: DanceStyleReviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.DanceStyleReviews
                .Include(r => r.DanceStyle)
                .Include(r => r.Schedule)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return NotFound();

            return View(review);
        }


        // //GET: DanceStyleReviews/Create?scheduleId=5
        //public IActionResult Create(int? scheduleId = null)
        //{
        //    var schedules = _context.Schedule
        //        .Include(s => s.ScheduleDanceStyles)
        //        .ThenInclude(sds => sds.DanceStyle)
        //        .ToList();

        //     //Здесь должен быть какой-то формат отображения расписания. Пример:
        //    ViewBag.Schedules = new SelectList(
        //        schedules.Select(s => new
        //        {
        //            Id = s.Id,
        //            Display = $"{s.DayOfWeek} {s.StartTime:hh\\:mm}"
        //        }),
        //        "Id",
        //        "Display",
        //        scheduleId
        //    );

        //    var review = new DanceStyleReview();
        //    if (scheduleId.HasValue)
        //    {
        //        var schedule = schedules.FirstOrDefault(s => s.Id == scheduleId.Value);
        //        if (schedule != null)
        //        {
        //            var danceStyle = schedule.ScheduleDanceStyles.Select(sds => sds.DanceStyle).FirstOrDefault();
        //            review.ScheduleId = schedule.Id;
        //            if (danceStyle != null)
        //                review.DanceStyleId = danceStyle.Id;
        //        }
        //    }

        //    return View(review);
        //}



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScheduleId,Rating,Comment")] DanceStyleReview danceStyleReview)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
                return Unauthorized();

            // Проверка, был ли пользователь записан на это занятие и дата уже прошла
            var wasRegistered = await _context.Registration.AnyAsync(r =>
                r.UserId == user.Id &&
                r.ScheduleId == danceStyleReview.ScheduleId &&
                r.Date.Date <= DateTime.Today
            );

            if (!wasRegistered)
                return BadRequest("Вы можете оставить отзыв только на занятия, которые посещали.");

            // Подставляем DanceStyleId на основе Schedule
            var schedule = await _context.Schedule
                .Include(s => s.ScheduleDanceStyles)
                .FirstOrDefaultAsync(s => s.Id == danceStyleReview.ScheduleId);

            var danceStyleId = schedule?.ScheduleDanceStyles.FirstOrDefault()?.DanceStyleId;
            if (danceStyleId == null)
                return BadRequest("Невозможно определить направление танца для выбранного занятия.");

            // Заполнение и сохранение отзыва
            danceStyleReview.DanceStyleId = danceStyleId.Value;
            danceStyleReview.UserId = user.Id;
            danceStyleReview.CreatedAt = DateTime.Now;

            _context.Add(danceStyleReview);
            await _context.SaveChangesAsync();

            // Успешный результат (можно вернуть JSON, если используешь fetch/AJAX)
            return Ok(new { success = true, message = "Отзыв успешно добавлен." });
        }



        // GET: DanceStyleReviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.DanceStyleReviews.FindAsync(id);
            if (review == null) return NotFound();

            return View(review);
        }

        // POST: DanceStyleReviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rating,Comment")] DanceStyleReview reviewInput)
        {
            var review = await _context.DanceStyleReviews.FindAsync(id);
            if (review == null) return NotFound();

            if (review.User.UserName != User.Identity.Name)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    review.Rating = reviewInput.Rating;
                    review.Comment = reviewInput.Comment;
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanceStyleReviewExists(id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(review);
        }

        // GET: DanceStyleReviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.DanceStyleReviews
                .Include(r => r.DanceStyle)
                .Include(r => r.Schedule)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // POST: DanceStyleReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.DanceStyleReviews.FindAsync(id);
            if (review != null)
            {
                _context.DanceStyleReviews.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DanceStyleReviewExists(int id)
        {
            return _context.DanceStyleReviews.Any(e => e.Id == id);
        }
    }
}
