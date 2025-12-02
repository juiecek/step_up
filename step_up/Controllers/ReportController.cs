using Microsoft.AspNetCore.Mvc;
using step_up.Models.ViewModels;
using step_up.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Font;
using iText.Layout.Properties;

namespace step_up.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> InstructorReport(int? id, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Instructors = await _context.Instructor.ToListAsync();

            if (id == null)
                return View();

            var instructor = await _context.Instructor
                .Include(i => i.Schedules)
                .Include(i => i.InstructorReviews)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instructor == null)
                return NotFound();

            var (totalScheduleDates, _) = GetSchedulesInPeriod(instructor.Schedules, startDate, endDate);

            var scheduleIds = instructor.Schedules.Select(s => s.Id).ToList();
            var registrationsQuery = _context.Registration.Where(r => scheduleIds.Contains(r.ScheduleId));

            if (startDate.HasValue && endDate.HasValue)
            {
                registrationsQuery = registrationsQuery
                    .Where(r => r.Date >= startDate.Value && r.Date <= endDate.Value);
            }

            var registrationCount = await registrationsQuery.CountAsync();
            var reviews = instructor.InstructorReviews.ToList();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            var model = new InstructorReportViewModel
            {
                Instructor = instructor,
                TotalSchedules = totalScheduleDates,
                TotalRegistrations = registrationCount,
                AverageRating = averageRating,
                Reviews = reviews,
                StartDate = startDate,
                EndDate = endDate
            };

            return View(model);
        }

        public async Task<IActionResult> ExportToPdf(int id, DateTime? startDate, DateTime? endDate)
        {
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
                return NotFound();

            var (totalSchedules, schedulesInPeriod) = GetSchedulesInPeriod(instructor.Schedules, startDate, endDate);

            var scheduleIds = instructor.Schedules.Select(s => s.Id).ToList();
            var registrationsQuery = _context.Registration.Where(r => scheduleIds.Contains(r.ScheduleId));

            if (startDate.HasValue && endDate.HasValue)
            {
                registrationsQuery = registrationsQuery
                    .Where(r => r.Date >= startDate && r.Date <= endDate);
            }

            var registrationCount = await registrationsQuery.CountAsync();
            var reviews = instructor.InstructorReviews.ToList();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            using (var ms = new MemoryStream())
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(writer))
            {
                var document = new Document(pdf);
                var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, true);

                // Шапка отчета
                // Шапка отчета
                document.Add(new Paragraph("Танцевальная студия \"Step Up\"")
                    .SetFont(font).SetFontSize(12).SetBold().SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("г. Минск, ул. Примерная, д. 12, тел. +375 (29) 123-45-67")
                    .SetFont(font).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\nОТЧЁТ")
                    .SetFont(font).SetFontSize(14).SetBold().SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph($"о преподавательской деятельности").SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph($"{instructor.FullName}")
                    .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER));

                if (startDate.HasValue && endDate.HasValue)
                {
                    document.Add(new Paragraph($"за период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}")
                        .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER));
                }


                document.Add(new Paragraph("\n").SetFont(font));

                // Основные данные
                document.Add(new Paragraph($"Телефон: {instructor.Phone}").SetFont(font));
                document.Add(new Paragraph($"Количество занятий: {totalSchedules}").SetFont(font));
                document.Add(new Paragraph($"Записей на занятия: {registrationCount}").SetFont(font));
                document.Add(new Paragraph($"Средний рейтинг: {averageRating:0.0}").SetFont(font).SetMarginBottom(10));

                // Занятия
                document.Add(new Paragraph("Занятия:").SetFont(font).SetBold());
                if (schedulesInPeriod.Any())
                {
                    foreach (var schedule in schedulesInPeriod)
                    {
                        var day = schedule.DayOfWeek.ToString();
                        var time = schedule.StartTime.ToString(@"hh\:mm");
                        document.Add(new Paragraph($"{day}, {time}").SetFont(font));

                        foreach (var sds in schedule.ScheduleDanceStyles)
                        {
                            var direction = sds.DanceStyle?.Name ?? "Без направления";
                            var level = sds.Level?.Name ?? "Без уровня";
                            document.Add(new Paragraph($"→ {direction}, уровень: {level}").SetFont(font).SetMarginLeft(20));
                        }
                    }
                }
                else
                {
                    document.Add(new Paragraph("Нет занятий за выбранный период.").SetFont(font));
                }

                // Отзывы
                document.Add(new Paragraph("\nОтзывы:").SetFont(font).SetBold());
                if (reviews.Any())
                {
                    foreach (var review in reviews)
                    {
                        document.Add(new Paragraph(
                            $"Оценка: {review.Rating}, Комментарий: {review.Comment}, Дата: {review.CreatedAt:dd.MM.yyyy}"
                        ).SetFont(font));
                    }
                }
                else
                {
                    document.Add(new Paragraph("Нет отзывов.").SetFont(font));
                }

                // Подвал с подписью
                document.Add(new Paragraph("\n\n").SetFont(font));
                document.Add(new Paragraph($"Дата составления отчёта: {DateTime.Now:dd.MM.yyyy}").SetFont(font));
                document.Add(new Paragraph("Составил: ___________________________").SetFont(font).SetMarginTop(30));
                document.Add(new Paragraph("              (должность, ФИО, подпись)").SetFont(font).SetFontSize(10));

                document.Close();
                var fileName = $"InstructorReport_{instructor.FullName.Replace(" ", "_")}.pdf";
                return File(ms.ToArray(), "application/pdf", fileName);
            }
        }


        // Метод для подсчёта занятий за период
        private (int Count, List<Schedules> Items) GetSchedulesInPeriod(IEnumerable<Schedules> schedules, DateTime? startDate, DateTime? endDate)
        {
            int count = 0;
            var list = new List<Schedules>();

            if (startDate.HasValue && endDate.HasValue)
            {
                foreach (var schedule in schedules)
                {
                    var current = startDate.Value.Date;
                    while (current <= endDate.Value.Date)
                    {
                        if (current.DayOfWeek == schedule.DayOfWeek)
                        {
                            count++;
                            list.Add(schedule);
                        }
                        current = current.AddDays(1);
                    }
                }
            }
            else
            {
                count = schedules.Count();
                list = schedules.ToList();
            }

            return (count, list);
        }
    }
}
