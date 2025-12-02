using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using step_up.Models;
using step_up.Models.ViewModels;

namespace step_up.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void PopulateDaysOfWeek()
        {
            ViewBag.DaysOfWeek = new List<SelectListItem>
            {
                new SelectListItem { Value = DayOfWeek.Monday.ToString(), Text = "Понедельник" },
                new SelectListItem { Value = DayOfWeek.Tuesday.ToString(), Text = "Вторник" },
                new SelectListItem { Value = DayOfWeek.Wednesday.ToString(), Text = "Среда" },
                new SelectListItem { Value = DayOfWeek.Thursday.ToString(), Text = "Четверг" },
                new SelectListItem { Value = DayOfWeek.Friday.ToString(), Text = "Пятница" },
                new SelectListItem { Value = DayOfWeek.Saturday.ToString(), Text = "Суббота" },
                new SelectListItem { Value = DayOfWeek.Sunday.ToString(), Text = "Воскресенье" },
            };
        }

        private static readonly Dictionary<DayOfWeek, string> RussianDayNames = new()
{
    { DayOfWeek.Monday, "Понедельник" },
    { DayOfWeek.Tuesday, "Вторник" },
    { DayOfWeek.Wednesday, "Среда" },
    { DayOfWeek.Thursday, "Четверг" },
    { DayOfWeek.Friday, "Пятница" },
    { DayOfWeek.Saturday, "Суббота" },
    { DayOfWeek.Sunday, "Воскресенье" }
};


        // GET: Schedules/Index
        public async Task<IActionResult> Index()
        {
            var schedules = await _context.Schedule
                .Include(s => s.Instructor)
                .Include(s => s.Hall)
                .Include(s => s.ScheduleDanceStyles)
                    .ThenInclude(sds => sds.DanceStyle)
                .Include(s => s.ScheduleDanceStyles)
                    .ThenInclude(sds => sds.Level)
                .ToListAsync();

            ViewBag.RussianDayNames = RussianDayNames;
            // Получаем все отзывы к этим занятиям (или фильтруй по какому-то критерию)
            var scheduleIds = schedules.Select(s => s.Id).ToList();

            var reviews = await _context.DanceStyleReviews
                .Include(r => r.User)
                .Include(r => r.DanceStyle)
                .Where(r => scheduleIds.Contains(r.ScheduleId))
                .ToListAsync();

            var viewModel = new ScheduleWithReviewsViewModel
            {
                Schedules = schedules,
                Reviews = reviews,
                NewReview = new DanceStyleReview() // пустой объект для формы
            };
            return View(schedules);
        }

        // GET: Schedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var schedule = await _context.Schedule
                .Include(s => s.Hall)
                .Include(s => s.Instructor)
                .Include(s => s.ScheduleDanceStyles)
                    .ThenInclude(sds => sds.Level)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (schedule == null)
                return NotFound();

            return View(schedule);
        }

        // GET: Schedules/Create
        public IActionResult Create()
        {
            ViewBag.Halls = _context.Hall.ToList();
            ViewBag.Instructors = _context.Instructor.ToList();
            ViewBag.Levels = _context.Levels.ToList();
            PopulateDaysOfWeek();

            return View();
        }

        // POST: Schedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,HallId,InstructorId,DayOfWeek,StartTime,Duration,MaxParticipants")] Schedules schedule,
            int levelId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Halls = _context.Hall.ToList();
                ViewBag.Instructors = _context.Instructor.ToList();
                ViewBag.Levels = _context.Levels.ToList();
                PopulateDaysOfWeek();
                return View(schedule);
            }

            _context.Add(schedule);
            await _context.SaveChangesAsync();

            var instructor = await _context.Instructor
                .FirstOrDefaultAsync(i => i.Id == schedule.InstructorId);

            if (instructor != null && instructor.DanceStyleId != null)
            {
                _context.ScheduleDanceStyles.Add(new ScheduleDanceStyle
                {
                    ScheduleId = schedule.Id,
                    DanceStyleId = instructor.DanceStyleId.Value,
                    LevelId = levelId
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var schedule = await _context.Schedule
                .Include(s => s.ScheduleDanceStyles)
                    .ThenInclude(sds => sds.DanceStyle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (schedule == null)
                return NotFound();

            ViewBag.Halls = _context.Hall.ToList();
            ViewBag.Instructors = _context.Instructor.ToList();
            ViewBag.DanceStyles = _context.DanceStyles.ToList();
            ViewBag.Levels = _context.Levels.ToList();

            var selectedDanceStyleIds = schedule.ScheduleDanceStyles
                .Select(sds => sds.DanceStyleId)
                .ToList();
            ViewBag.SelectedDanceStyles = selectedDanceStyleIds;

            var selectedLevelId = schedule.ScheduleDanceStyles
                .FirstOrDefault()?.LevelId;
            ViewBag.SelectedLevelId = selectedLevelId;

            PopulateDaysOfWeek();

            return View(schedule);
        }

        // POST: Schedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,InstructorId,HallId,DayOfWeek,StartTime,Duration,MaxParticipants")] Schedules schedule,
            int? LevelId)
        {
            if (id != schedule.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);

                    var existingStyles = await _context.ScheduleDanceStyles
                        .Where(s => s.ScheduleId == schedule.Id)
                        .ToListAsync();

                    _context.ScheduleDanceStyles.RemoveRange(existingStyles);

                    var instructor = await _context.Instructor
                        .FirstOrDefaultAsync(i => i.Id == schedule.InstructorId);

                    if (instructor != null && instructor.DanceStyleId != null && LevelId.HasValue)
                    {
                        _context.ScheduleDanceStyles.Add(new ScheduleDanceStyle
                        {
                            ScheduleId = schedule.Id,
                            DanceStyleId = instructor.DanceStyleId.Value,
                            LevelId = LevelId.Value
                        });
                    }


                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Halls = _context.Hall.ToList();
            ViewBag.Instructors = _context.Instructor.ToList();
            ViewBag.DanceStyles = _context.DanceStyles.ToList();
            ViewBag.Levels = _context.Levels.ToList();
            ViewBag.SelectedLevelId = LevelId;
            PopulateDaysOfWeek();
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var schedule = await _context.Schedule
                .Include(s => s.Hall)
                .Include(s => s.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (schedule == null)
                return NotFound();

            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedule.FindAsync(id);
            if (schedule == null)
                return NotFound();

            _context.Schedule.Remove(schedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedule.Any(e => e.Id == id);
        }
    }
}
