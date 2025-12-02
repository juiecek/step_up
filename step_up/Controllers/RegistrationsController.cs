using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using step_up.Models;

namespace step_up.Controllers
{
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Registrations
        public async Task<IActionResult> Index()
        {
            var registrations = _context.Registration
                .Include(r => r.Schedule).ThenInclude(s => s.Instructor)
                .Include(r => r.User)
                .Include(r => r.UserSubscription).ThenInclude(us => us.Subscription);

            return View(await registrations.ToListAsync());
        }

        // GET: Registrations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var registration = await _context.Registration
                .Include(r => r.Schedule).ThenInclude(s => s.Instructor)
                .Include(r => r.User)
                .Include(r => r.UserSubscription).ThenInclude(us => us.Subscription)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null) return NotFound();

            return View(registration);
        }

        // GET: Registrations/Create
        [Authorize]
        public IActionResult Create()
        {
            var currentUserId = _context.User.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            if (currentUserId == null) return Unauthorized();

            ViewBag.CurrentUserId = currentUserId;

            var subscriptions = _context.UserSubscriptions
                .Include(us => us.Subscription)
                .Where(us =>
                    us.UserId == currentUserId &&
                    us.ExpiryDate >= DateTime.Today &&
                    (us.Subscription.NumberOfClasses == 0 || us.ClassesRemaining > 0)
                )
                .ToList();

            ViewBag.UserSubscriptionId = new SelectList(
                subscriptions.Select(us => new
                {
                    us.Id,
                    DisplayName = $"{us.Subscription.Name} (до {us.ExpiryDate:dd.MM.yyyy}, осталось занятий: {(us.Subscription.NumberOfClasses == 0 ? "∞" : us.ClassesRemaining.ToString())})"
                }),
                "Id",
                "DisplayName"
            );

            var schedules = _context.Schedule
                .Include(s => s.Instructor)
                .Include(s => s.Hall)
                .ToList();

            ViewBag.ScheduleId = new SelectList(
                schedules.Select(s => new
                {
                    s.Id,
                    Display = $"{s.Instructor.FullName} — {s.DayOfWeek} {s.StartTime:hh\\:mm} — {s.Hall.Name}"
                }),
                "Id",
                "Display"
            );

            return View();
        }

        // POST: Registrations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScheduleId,UserSubscriptionId,Date")] Registration registration)
        {
            var currentUserId = _context.User.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            if (currentUserId == null)
                return Unauthorized();

            registration.UserId = currentUserId;
            ModelState.Remove(nameof(registration.UserId));

            if (ModelState.IsValid)
            {
                var userSub = await _context.UserSubscriptions
                    .Include(us => us.Subscription)
                    .FirstOrDefaultAsync(us => us.Id == registration.UserSubscriptionId && us.UserId == currentUserId);

                var schedule = await _context.Schedule.FirstOrDefaultAsync(s => s.Id == registration.ScheduleId);

                if (userSub == null)
                {
                    ModelState.AddModelError("", "Абонемент не найден.");
                }
                else if (userSub.ExpiryDate < registration.Date)
                {
                    ModelState.AddModelError("", $"Абонемент действует до {userSub.ExpiryDate:dd.MM.yyyy}, а дата занятия — {registration.Date:dd.MM.yyyy}.");
                }
                else if (userSub.ClassesRemaining <= 0 && userSub.Subscription.NumberOfClasses > 0)
                {
                    ModelState.AddModelError("", "У вас закончились занятия в абонементе.");
                }
                else if (schedule == null || schedule.DayOfWeek != registration.Date.DayOfWeek)
                {
                    ModelState.AddModelError("", "Дата не соответствует дню недели занятия.");
                }
                else
                {
                    var existing = await _context.Registration
                        .FirstOrDefaultAsync(r => r.UserId == currentUserId &&
                                                  r.ScheduleId == registration.ScheduleId &&
                                                  r.Date == registration.Date);
                    if (existing != null)
                    {
                        ModelState.AddModelError("", "Вы уже записаны на это занятие.");
                    }
                    else
                    {
                        var count = await _context.Registration
                            .CountAsync(r => r.ScheduleId == registration.ScheduleId && r.Date == registration.Date);
                        if (count >= schedule.MaxParticipants)
                        {
                            ModelState.AddModelError("", "На это занятие нет свободных мест.");
                        }
                        else
                        {
                            _context.Registration.Add(registration);
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = "Вы успешно записались!";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }

            // Повторная инициализация ViewBag
            var subs = _context.UserSubscriptions
                .Include(us => us.Subscription)
                .Where(us =>
                    us.UserId == currentUserId &&
                    us.ExpiryDate >= DateTime.Today &&
                    (us.Subscription.NumberOfClasses == 0 || us.ClassesRemaining > 0)
                )
                .ToList();

            ViewBag.UserSubscriptionId = new SelectList(
                subs.Select(us => new
                {
                    us.Id,
                    DisplayName = $"{us.Subscription.Name} (до {us.ExpiryDate:dd.MM.yyyy}, осталось занятий: {(us.Subscription.NumberOfClasses == 0 ? "∞" : us.ClassesRemaining.ToString())})"
                }),
                "Id",
                "DisplayName",
                registration.UserSubscriptionId
            );

            var schedules = _context.Schedule
                .Include(s => s.Instructor)
                .Include(s => s.Hall)
                .ToList();

            ViewBag.ScheduleId = new SelectList(
                schedules.Select(s => new
                {
                    s.Id,
                    Display = $"{s.Instructor.FullName} — {s.DayOfWeek} {s.StartTime:hh\\:mm} — {s.Hall.Name}"
                }),
                "Id",
                "Display",
                registration.ScheduleId
            );

            return View(registration);
        }


        // ❗AJAX метод
        [HttpGet]
        public IActionResult GetAvailableSpots(int scheduleId, DateTime date)
        {
            var schedule = _context.Schedule.FirstOrDefault(s => s.Id == scheduleId);
            if (schedule == null)
                return Json(new { success = false, message = "Расписание не найдено" });

            var currentCount = _context.Registration
                .Count(r => r.ScheduleId == scheduleId && r.Date == date);

            var available = schedule.MaxParticipants - currentCount;

            return Json(new { success = true, available });
        }



        // GET: Registrations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var registration = await _context.Registration.FindAsync(id);
            if (registration == null) return NotFound();

            ViewBag.ScheduleId = new SelectList(_context.Schedule.Include(s => s.Instructor), "Id", "Instructor.FullName", registration.ScheduleId);
            ViewBag.UserSubscriptionId = new SelectList(_context.UserSubscriptions.Include(us => us.Subscription), "Id", "Subscription.Name", registration.UserSubscriptionId);
            ViewBag.UserId = new SelectList(_context.User, "Id", "UserName", registration.UserId);

            return View(registration);
        }

        // POST: Registrations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ScheduleId,Date,RegistrationDate,UserSubscriptionId,Attended")] Registration registration)
        {
            if (id != registration.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registration);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Registration.Any(e => e.Id == registration.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.ScheduleId = new SelectList(_context.Schedule.Include(s => s.Instructor), "Id", "Instructor.FullName", registration.ScheduleId);
            ViewBag.UserSubscriptionId = new SelectList(_context.UserSubscriptions.Include(us => us.Subscription), "Id", "Subscription.Name", registration.UserSubscriptionId);
            ViewBag.UserId = new SelectList(_context.User, "Id", "UserName", registration.UserId);

            return View(registration);
        }

        // GET: Registrations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var registration = await _context.Registration
                .Include(r => r.Schedule).ThenInclude(s => s.Instructor)
                .Include(r => r.User)
                .Include(r => r.UserSubscription).ThenInclude(us => us.Subscription)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (registration == null) return NotFound();

            return View(registration);
        }

        // POST: Registrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var registration = await _context.Registration.FindAsync(id);
            if (registration != null)
            {
                _context.Registration.Remove(registration);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ❗ Возвращает список возможных дат занятий
        [HttpGet]
        public IActionResult GetScheduleDates(int scheduleId)
        {
            var schedule = _context.Schedule.FirstOrDefault(s => s.Id == scheduleId);
            if (schedule == null)
                return Json(new { success = false });

            var today = DateTime.Today;
            var result = new List<string>();

            for (int i = 0; i < 28; i++)
            {
                var date = today.AddDays(i);
                if (date.DayOfWeek == schedule.DayOfWeek)
                {
                    result.Add(date.ToString("yyyy-MM-dd"));
                }
            }

            return Json(new { success = true, dates = result });
        }


    }
}
