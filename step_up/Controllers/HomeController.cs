using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using step_up.Models;
using System.Diagnostics;
using System.Security.Claims;
using step_up.Models.ViewModels;
using step_up.ViewModels;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var subscriptions = _context.Subscriptions.ToList();
        return View(subscriptions);
    }

    public IActionResult History()
    {
        return View();
    }

    public async Task<IActionResult> Teachers()
    {
        try
        {
            var instructors = await _context.Instructor.ToListAsync(); // Асинхронная загрузка данных
            var reviews = await _context.InstructorReviews
       .Include(r => r.User)
       .ToListAsync();

            ViewBag.Reviews = reviews;
            return View(instructors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке данных инструктора.");
            return View("Error");
        }
    }




    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public IActionResult Schedule(string selectedStyle = null, string selectedLevel = null)
    {
        // Получаем расписание с необходимыми включениями
        var schedule = _context.Schedule
            .Include(s => s.ScheduleDanceStyles).ThenInclude(sd => sd.DanceStyle)
            .Include(s => s.ScheduleDanceStyles).ThenInclude(sd => sd.Level)
            .Include(s => s.Instructor)
            .AsQueryable();

        // Фильтрация по стилю
        if (!string.IsNullOrEmpty(selectedStyle))
        {
            schedule = schedule.Where(s => s.ScheduleDanceStyles.Any(sd => sd.DanceStyle.Name == selectedStyle));
        }

        // Фильтрация по уровню
        if (!string.IsNullOrEmpty(selectedLevel))
        {
            schedule = schedule.Where(s => s.ScheduleDanceStyles.Any(sd => sd.Level.Name == selectedLevel));
        }

        // Группировка по StartTime
        var groupedSchedule = schedule
            .ToList()
            .GroupBy(s => s.StartTime)
            .OrderBy(g => g.Key)
            .ToList();

        // Фильтры для выбора в интерфейсе
        var danceStyles = _context.DanceStyles
            .Select(ds => new SelectListItem
            {
                Text = ds.Name,
                Value = ds.Name
            }).ToList();

        var levels = _context.Levels
            .Select(l => new SelectListItem
            {
                Text = l.Name,
                Value = l.Name
            }).ToList();

        ViewData["DanceStyleFilter"] = new SelectList(danceStyles, "Value", "Text");
        ViewData["LevelFilter"] = new SelectList(levels, "Value", "Text");
        ViewData["SelectedStyle"] = selectedStyle;
        ViewData["SelectedLevel"] = selectedLevel;

        // Загружаем последние отзывы для отображения
        var reviews = _context.DanceStyleReviews
            .Include(r => r.User)
            .Include(r => r.DanceStyle)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .ToList();

        ViewBag.Reviews = reviews;

        // Заполняем ViewBag для выпадающих списков в форме отзыва

        // Список занятий (ScheduleId и дата/время для отображения)
        var schedulesList = _context.Schedule
        .Include(s => s.Instructor)
        .ToList()
        .Select(s => new
        {
            s.Id,
            Display = $"{s.DayOfWeek} {s.StartTime:hh\\:mm} - {s.Instructor.FullName}"
        })
        .ToList();

        ViewBag.Schedules = new SelectList(schedulesList, "Id", "Display");


       
        return View(groupedSchedule);
    }




    public async Task<IActionResult> Halls()
    {
        try
        {
            var halls = await _context.Hall.ToListAsync();
            return View(halls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке данных залов.");
            return View("Error");
        }
    }

    public IActionResult Registration()
    {
        return View();
    }
    public IActionResult Record()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }
    public IActionResult DanceStyles()
    {
        return View();
    }


    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Получаем идентификатор пользователя
        var userSubscriptions = await _context.UserSubscriptions
                                              .Include(us => us.Subscription)
                                              .Where(us => us.UserId == userId) // Фильтруем по текущему пользователю
                                              .ToListAsync();

        ViewBag.UserSubscriptions = userSubscriptions; // Передаем список абонементов через ViewBag

        return View();
    }

    public async Task<IActionResult> InstructorReport(int? id, DateTime? startDate, DateTime? endDate)
    {
        var instructors = await _context.Instructor.ToListAsync();
        ViewBag.Instructors = instructors;

        if (id == null)
        {
            return View(); // Показываем форму выбора преподавателя
        }

        var instructor = await _context.Instructor
       .Include(i => i.Schedules)
           .ThenInclude(s => s.ScheduleDanceStyles)
               .ThenInclude(sds => sds.DanceStyle)
       .Include(i => i.Schedules)
           .ThenInclude(s => s.ScheduleDanceStyles)
               .ThenInclude(sds => sds.Level)
       .Include(i => i.InstructorReviews)
       .FirstOrDefaultAsync(i => i.Id == id);

        if (instructor == null)
        {
            return NotFound();
        }

        // Подсчёт количества занятий за период
        int totalSchedules = 0;
        var schedulesInPeriod = new List<Schedules>();

        if (startDate.HasValue && endDate.HasValue)
        {
            foreach (var schedule in instructor.Schedules)
            {
                var current = startDate.Value.Date;
                while (current <= endDate.Value.Date)
                {
                    if (current.DayOfWeek == schedule.DayOfWeek)
                    {
                        totalSchedules++;
                        schedulesInPeriod.Add(schedule);
                    }
                    current = current.AddDays(1);
                }
            }
        }
        else
        {
            totalSchedules = instructor.Schedules.Count;
            schedulesInPeriod = instructor.Schedules.ToList();
        }

        // Подсчёт количества записей за период
        var scheduleIds = instructor.Schedules.Select(s => s.Id).ToList();
        var registrationsQuery = _context.Registration
            .Where(r => scheduleIds.Contains(r.ScheduleId));

        if (startDate.HasValue && endDate.HasValue)
        {
            registrationsQuery = registrationsQuery
                .Where(r => r.Date >= startDate.Value && r.Date <= endDate.Value);
        }

        var totalRegistrations = await registrationsQuery.CountAsync();

        // Отзывы и средняя оценка
        var reviews = instructor.InstructorReviews.ToList();
        var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

        var model = new InstructorReportViewModel
        {
            Instructor = instructor,
            TotalSchedules = totalSchedules,
            TotalRegistrations = totalRegistrations,
            AverageRating = averageRating,
            Reviews = reviews,
            StartDate = startDate,
            EndDate = endDate,
            SchedulesInPeriod = schedulesInPeriod
        };

        return View(model);
    }




    public async Task<IActionResult> GroupFillingReport(int? danceStyleId, int? levelId, int? instructorId, DateTime? dateFrom, DateTime? dateTo)
    {
        // Запрос с подгрузкой нужных данных
        var query = _context.Schedule
            .Include(s => s.ScheduleDanceStyles)
                .ThenInclude(sds => sds.DanceStyle)
            .Include(s => s.ScheduleDanceStyles)
                .ThenInclude(sds => sds.Level)
            .Include(s => s.Registrations)
            .Include(s => s.Instructor)
            .AsQueryable();

        if (danceStyleId.HasValue)
            query = query.Where(s => s.ScheduleDanceStyles.Any(sds => sds.DanceStyleId == danceStyleId));

        if (levelId.HasValue)
            query = query.Where(s => s.ScheduleDanceStyles.Any(sds => sds.LevelId == levelId));

        if (instructorId.HasValue)
            query = query.Where(s => s.InstructorId == instructorId);

        // Получаем все расписания, удовлетворяющие фильтрам
        var schedules = await query.ToListAsync();

        var reportItems = schedules.SelectMany(s =>
        {
            // Фильтруем регистрации по дате, если указаны
            var filteredRegistrations = s.Registrations.AsQueryable();

            if (dateFrom.HasValue)
                filteredRegistrations = filteredRegistrations.Where(r => r.Date >= dateFrom.Value);

            if (dateTo.HasValue)
                filteredRegistrations = filteredRegistrations.Where(r => r.Date <= dateTo.Value);

            int currentCount = filteredRegistrations.Count();

            return s.ScheduleDanceStyles.Select(sds => new GroupFillingReportItem
            {
                GroupName = $"{s.DayOfWeek}, {s.StartTime:hh\\:mm}", // Можно настроить отображение иначе
                DanceStyleName = sds.DanceStyle?.Name ?? "Не указано",
                LevelName = sds.Level?.Name ?? "Не указано",
                MaxCapacity = s.MaxParticipants,
                CurrentCount = currentCount,
                FillingPercent = s.MaxParticipants > 0 ? (double)currentCount / s.MaxParticipants * 100 : 0
            });
        }).ToList();

        var viewModel = new GroupFillingReportViewModel
        {
            Groups = reportItems,
            DanceStyles = await _context.DanceStyles.ToListAsync(),
            Levels = await _context.Levels.ToListAsync(),
            Instructors = await _context.Instructor.ToListAsync(),
            SelectedDanceStyleId = danceStyleId,
            SelectedLevelId = levelId,
            SelectedInstructorId = instructorId,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        return View("~/Views/Report/GroupFillingReport.cshtml", viewModel);
    }



    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ClientActivityReport(string activityLevel)
    {
        // Загружаем пользователей с регистрациями и расписаниями
        var users = await _context.Users
            .Include(u => u.Registrations)
                .ThenInclude(r => r.Schedule)
            .ToListAsync();

        var userActivityReports = users.Select(u =>
        {
            var attendedRegistrations = u.Registrations
                .Where(r => r.Attended)
                .ToList();

            return new ClientActivityReport
            {
                FullName = u.FullName,
                RegistrationDate = u.CreatedAt,
                TotalAttendances = attendedRegistrations.Count,
                LastAttendanceDate = attendedRegistrations
                    .OrderByDescending(r => r.Date) // 📌 Используем r.Date вместо r.Schedule.Date
                    .Select(r => r.Date)
                    .FirstOrDefault(),
                ActivityLevel = GetActivityLevel(attendedRegistrations.Count)
            };
        }).ToList();

        // Фильтрация по уровню активности, если задано
        if (!string.IsNullOrEmpty(activityLevel))
        {
            userActivityReports = userActivityReports
                .Where(r => r.ActivityLevel == activityLevel)
                .ToList();
        }

        var activityLevels = new List<string> { "Низкая", "Средняя", "Высокая" };

        var viewModel = new ClientActivityReportViewModel
        {
            UserActivityReports = userActivityReports,
            ActivityLevels = activityLevels,
            ActivityLevel = activityLevel
        };

        return View("~/Views/Report/ClientActivityReport.cshtml", viewModel);
    }


    private string GetActivityLevel(int attendedCount)
    {
        if (attendedCount >= 21)
            return "Высокая";
        if (attendedCount >= 10)
            return "Средняя";
        return "Низкая";
    }



    [Authorize(Roles = "Admin")]
    public IActionResult AdminInstructor()
    {
        return RedirectToAction("Index", "Instructors");
    }
    [Authorize(Roles = "Admin")]
    public IActionResult AdminLevel()
    {
        return RedirectToAction("Index", "Levels");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminHall()
    {
        return RedirectToAction("Index", "Halls");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminSchedule()
    {
        return RedirectToAction("Index", "Schedules");
    }
    [Authorize(Roles = "Admin")]
    public IActionResult AdminSubsription()
    {
        return RedirectToAction("Index", "Subscriptions");
    }

    public IActionResult InstructorView()
    {
        return RedirectToAction("InstructorView", "Instructors");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminDanceStyle()
    {
        return RedirectToAction("Index", "DanceStyles");
    }
    [Authorize(Roles = "Admin")]
    public IActionResult AdminUserSubscription()
    {
        return RedirectToAction("Index", "UserSubscriptions");
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
