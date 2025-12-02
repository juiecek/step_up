using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using step_up.Models;
using step_up.Models.ViewModels;
using iText.Layout;
using iText.IO.Font.Constants;
using iText.IO;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace step_up.Controllers
{
    public class GroupFillingReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupFillingReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? danceStyleId, int? levelId, int? instructorId, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.Schedule
                .Include(s => s.ScheduleDanceStyles).ThenInclude(sds => sds.DanceStyle)
                .Include(s => s.ScheduleDanceStyles).ThenInclude(sds => sds.Level)
                .Include(s => s.Registrations)
                .Include(s => s.Instructor)
                .AsQueryable();

            if (danceStyleId.HasValue)
                query = query.Where(s => s.ScheduleDanceStyles.Any(sds => sds.DanceStyleId == danceStyleId));

            if (levelId.HasValue)
                query = query.Where(s => s.ScheduleDanceStyles.Any(sds => sds.LevelId == levelId));

            if (instructorId.HasValue)
                query = query.Where(s => s.InstructorId == instructorId);

            // Важно: фильтровать расписания, у которых есть записи по дате из фильтра
            if (dateFrom.HasValue)
                query = query.Where(s => s.Registrations.Any(r => r.Date >= dateFrom.Value));

            if (dateTo.HasValue)
                query = query.Where(s => s.Registrations.Any(r => r.Date <= dateTo.Value));

            var schedules = await query.ToListAsync();

            var reportItems = new List<GroupFillingReportItem>();

            foreach (var schedule in schedules)
            {
                // Берём записи по фильтру дат (если есть)
                var relevantRegistrations = schedule.Registrations.AsQueryable();

                if (dateFrom.HasValue)
                    relevantRegistrations = relevantRegistrations.Where(r => r.Date >= dateFrom.Value);

                if (dateTo.HasValue)
                    relevantRegistrations = relevantRegistrations.Where(r => r.Date <= dateTo.Value);

                var registrationsByDate = relevantRegistrations
                    .GroupBy(r => r.Date.Date) // группируем по дате (без времени)
                    .ToList();

                // Если нет записей, добавим группу с 0 заполнением хотя бы один раз для наглядности
                if (!registrationsByDate.Any())
                {
                    foreach (var sds in schedule.ScheduleDanceStyles)
                    {
                        reportItems.Add(new GroupFillingReportItem
                        {
                            GroupName = $"{schedule.DayOfWeek}, {schedule.StartTime:hh\\:mm}",
                            DanceStyleName = sds.DanceStyle?.Name ?? "Не указано",
                            LevelName = sds.Level?.Name ?? "Не указано",
                            MaxCapacity = schedule.MaxParticipants,
                            CurrentCount = 0,
                            FillingPercent = 0
                        });
                    }
                }
                else
                {
                    foreach (var dateGroup in registrationsByDate)
                    {
                        foreach (var sds in schedule.ScheduleDanceStyles)
                        {
                            int currentCount = dateGroup.Count();
                            double fillingPercent = schedule.MaxParticipants > 0
                                ? (double)currentCount / schedule.MaxParticipants * 100
                                : 0;

                            // Ограничиваем максимум 100%
                            if (fillingPercent > 100) fillingPercent = 100;

                            reportItems.Add(new GroupFillingReportItem
                            {
                                GroupName = $"{schedule.DayOfWeek}, {schedule.StartTime:hh\\:mm} ({dateGroup.Key:dd.MM.yyyy})",
                                DanceStyleName = sds.DanceStyle?.Name ?? "Не указано",
                                LevelName = sds.Level?.Name ?? "Не указано",
                                MaxCapacity = schedule.MaxParticipants,
                                CurrentCount = currentCount,
                                FillingPercent = fillingPercent
                            });
                        }
                    }
                }
            }

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

            return View(viewModel);
        }


        public async Task<IActionResult> ExportGroupsToExcel(int? danceStyleId, int? levelId)
        {
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

            var schedules = await query.ToListAsync();

            var groupedByInstructor = schedules.GroupBy(s => s.Instructor).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                foreach (var group in groupedByInstructor)
                {
                    var instructor = group.Key;
                    string sheetName = instructor != null ? instructor.FullName : "Без преподавателя";
                    var worksheet = package.Workbook.Worksheets.Add(sheetName.Length > 31 ? sheetName.Substring(0, 31) : sheetName);

                    worksheet.Cells[1, 1].Value = "Группа";
                    worksheet.Cells[1, 2].Value = "Направление";
                    worksheet.Cells[1, 3].Value = "Уровень";
                    worksheet.Cells[1, 4].Value = "Макс. вместимость";
                    worksheet.Cells[1, 5].Value = "Текущее количество";
                    worksheet.Cells[1, 6].Value = "Заполненность (%)";

                    int row = 2;

                    foreach (var schedule in group)
                    {
                        // Группируем записи по дате
                        var registrationsByDate = schedule.Registrations
                            .GroupBy(r => r.Date.Date)
                            .ToList();

                        if (!registrationsByDate.Any())
                        {
                            foreach (var sds in schedule.ScheduleDanceStyles)
                            {
                                worksheet.Cells[row, 1].Value = $"{schedule.DayOfWeek}, {schedule.StartTime:hh\\:mm}";
                                worksheet.Cells[row, 2].Value = sds.DanceStyle?.Name ?? "Не указано";
                                worksheet.Cells[row, 3].Value = sds.Level?.Name ?? "Не указано";
                                worksheet.Cells[row, 4].Value = schedule.MaxParticipants;
                                worksheet.Cells[row, 5].Value = 0;
                                worksheet.Cells[row, 6].Value = "0.0";
                                row++;
                            }
                        }
                        else
                        {
                            foreach (var dateGroup in registrationsByDate)
                            {
                                foreach (var sds in schedule.ScheduleDanceStyles)
                                {
                                    int currentCount = dateGroup.Count();
                                    double fillPercent = schedule.MaxParticipants > 0
                                        ? (double)currentCount / schedule.MaxParticipants * 100
                                        : 0;

                                    if (fillPercent > 100) fillPercent = 100;

                                    worksheet.Cells[row, 1].Value = $"{schedule.DayOfWeek}, {schedule.StartTime:hh\\:mm} ({dateGroup.Key:dd.MM.yyyy})";
                                    worksheet.Cells[row, 2].Value = sds.DanceStyle?.Name ?? "Не указано";
                                    worksheet.Cells[row, 3].Value = sds.Level?.Name ?? "Не указано";
                                    worksheet.Cells[row, 4].Value = schedule.MaxParticipants;
                                    worksheet.Cells[row, 5].Value = currentCount;
                                    worksheet.Cells[row, 6].Value = fillPercent.ToString("0.0");

                                    row++;
                                }
                            }
                        }
                    }
                }

                var file = package.GetAsByteArray();
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GroupFillingByInstructor.xlsx");
            }
        }


        public async Task<IActionResult> ExportGroupsToPdf(int? danceStyleId, int? levelId)
        {
            var query = _context.Schedule
                .Include(s => s.ScheduleDanceStyles)
                    .ThenInclude(sds => sds.DanceStyle)
                .Include(s => s.ScheduleDanceStyles)
                    .ThenInclude(sds => sds.Level)
                .Include(s => s.Registrations)
                .AsQueryable();

            if (danceStyleId.HasValue)
                query = query.Where(s => s.ScheduleDanceStyles.Any(sds => sds.DanceStyleId == danceStyleId));

            if (levelId.HasValue)
                query = query.Where(s => s.ScheduleDanceStyles.Any(sds => sds.LevelId == levelId));

            var schedules = await query.ToListAsync();

            var reportItems = new List<GroupFillingReportItem>();

            foreach (var schedule in schedules)
            {
                var registrationsByDate = schedule.Registrations
                    .GroupBy(r => r.Date.Date)
                    .ToList();

                if (!registrationsByDate.Any())
                {
                    foreach (var sds in schedule.ScheduleDanceStyles)
                    {
                        reportItems.Add(new GroupFillingReportItem
                        {
                            GroupName = $"{schedule.DayOfWeek}, {schedule.StartTime:hh\\:mm}",
                            DanceStyleName = sds.DanceStyle?.Name ?? "Не указано",
                            LevelName = sds.Level?.Name ?? "Не указано",
                            MaxCapacity = schedule.MaxParticipants,
                            CurrentCount = 0,
                            FillingPercent = 0
                        });
                    }
                }
                else
                {
                    foreach (var dateGroup in registrationsByDate)
                    {
                        foreach (var sds in schedule.ScheduleDanceStyles)
                        {
                            int currentCount = dateGroup.Count();
                            double fillingPercent = schedule.MaxParticipants > 0
                                ? (double)currentCount / schedule.MaxParticipants * 100
                                : 0;

                            if (fillingPercent > 100) fillingPercent = 100;

                            reportItems.Add(new GroupFillingReportItem
                            {
                                GroupName = $"{schedule.DayOfWeek}, {schedule.StartTime:hh\\:mm} ({dateGroup.Key:dd.MM.yyyy})",
                                DanceStyleName = sds.DanceStyle?.Name ?? "Не указано",
                                LevelName = sds.Level?.Name ?? "Не указано",
                                MaxCapacity = schedule.MaxParticipants,
                                CurrentCount = currentCount,
                                FillingPercent = fillingPercent
                            });
                        }
                    }
                }
            }

            // Формируем PDF
            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Добавим заголовок
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                document.Add(new Paragraph("Отчет по заполнению групп")
                    .SetFont(font)
                    .SetFontSize(16)
                    .SetMarginBottom(20));

                var table = new Table(6, false);

                // Заголовки столбцов
                table.AddHeaderCell("Группа");
                table.AddHeaderCell("Направление");
                table.AddHeaderCell("Уровень");
                table.AddHeaderCell("Макс. вместимость");
                table.AddHeaderCell("Текущ. кол-во");
                table.AddHeaderCell("Заполненность (%)");

                foreach (var item in reportItems)
                {
                    table.AddCell(item.GroupName);
                    table.AddCell(item.DanceStyleName);
                    table.AddCell(item.LevelName);
                    table.AddCell(item.MaxCapacity.ToString());
                    table.AddCell(item.CurrentCount.ToString());
                    table.AddCell(item.FillingPercent.ToString("0.0"));
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", "GroupFillingReport.pdf");
            }
        }
    }
}
