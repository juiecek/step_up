using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using step_up.ViewModels;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using step_up.Models.ViewModels;
using iText.IO.Font;
using Microsoft.AspNetCore.Identity;
using step_up.Models;
using OfficeOpenXml.Style;



namespace step_up.Controllers
{
    public class ClientActivityReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<User> _userManager;

        public ClientActivityReportController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // Метод для просмотра отчета
        public async Task<IActionResult> Index(string? activityLevel)
        {
            var model = await GetClientActivityReports(activityLevel); // Используем await
            return View("~/Views/Report/ClientActivityReport.cshtml", model);
        }

        // Метод для получения отчетов клиентов (для использования в разных местах)
        private async Task<ClientActivityReportViewModel> GetClientActivityReports(string? activityLevel)
        {
            var users = await _context.Users
                .Include(u => u.Registrations)
                .ThenInclude(r => r.Schedule)
                .ToListAsync();

            var clients = new List<ClientActivityReport>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (roles.Contains("Admin"))
                    continue;

                var attendedRegistrations = u.Registrations
                    .Where(r => r.Attended)
                    .ToList();

                var lastAttendance = attendedRegistrations
                    .OrderByDescending(r => r.Date)
                    .Select(r => r.Date)
                    .FirstOrDefault();

                var attendanceCount = attendedRegistrations.Count;

                var level = attendanceCount >= 21 ? "Высокая"
                           : attendanceCount >= 10 ? "Средняя"
                           : "Низкая";

                clients.Add(new ClientActivityReport
                {
                    FullName = u.FullName,
                    Email = u.Email,
                    RegistrationDate = u.CreatedAt,
                    TotalAttendances = attendanceCount,
                    LastAttendanceDate = lastAttendance,
                    ActivityLevel = level
                });
            }

            // Фильтрация по уровню активности
            if (!string.IsNullOrEmpty(activityLevel))
            {
                clients = clients
                    .Where(c => c.ActivityLevel == activityLevel)
                    .ToList();
            }

            return new ClientActivityReportViewModel
            {
                UserActivityReports = clients,
                ActivityLevel = activityLevel,
                ActivityLevels = new List<string> { "Низкая", "Средняя", "Высокая" } // не забудь добавить в модель
            };
        }


        // Метод для экспорта в Excel
        public async Task<IActionResult> ExportToExcel(string? activityLevel, DateTime? startDate = null, DateTime? endDate = null)
        {
            var report = await GetClientActivityReports(activityLevel);
            var clients = report.UserActivityReports;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            // === Первый раздел: данные по активности ===
            var activityGroups = clients.GroupBy(c => c.ActivityLevel).OrderBy(g => g.Key);

            foreach (var group in activityGroups)
            {
                var worksheet = package.Workbook.Worksheets.Add($"Активность: {group.Key}");

                var headers = new[] { "№", "ФИО", "Дата регистрации", "Посещено", "Последнее посещение", "Уровень активности" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                int counter = 1;
                foreach (var client in group)
                {
                    worksheet.Cells[row, 1].Value = counter;
                    worksheet.Cells[row, 2].Value = string.IsNullOrWhiteSpace(client.FullName) ? client.Email : client.FullName;
                    worksheet.Cells[row, 3].Value = client.RegistrationDate.ToShortDateString();
                    worksheet.Cells[row, 4].Value = client.TotalAttendances;
                    worksheet.Cells[row, 5].Value = client.LastAttendanceDate?.ToShortDateString() ?? "-";
                    worksheet.Cells[row, 6].Value = client.ActivityLevel;

                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, headers.Length].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, headers.Length].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 240, 240));
                    }

                    row++;
                    counter++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells[row, 1].Value = "Итого клиентов:";
                worksheet.Cells[row, 2].Value = group.Count();
                worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
            }

            // === Второй раздел: динамика активности ===
            var users = await _context.Users.Include(u => u.Registrations).ToListAsync();
            var transitions = new Dictionary<(string from, string to), int>();

            string GetLevel(int count) => count >= 21 ? "Высокая" : count >= 10 ? "Средняя" : "Низкая";

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) continue;

                int beforeCount = user.Registrations.Count(r => r.Attended && r.Date < startDate);
                int afterCount = user.Registrations.Count(r => r.Attended && r.Date <= endDate);

                var fromLevel = GetLevel(beforeCount);
                var toLevel = GetLevel(afterCount);

                if (fromLevel != toLevel)
                {
                    var key = (fromLevel, toLevel);
                    if (!transitions.ContainsKey(key))
                        transitions[key] = 0;

                    transitions[key]++;
                }
            }

            var dynamicsSheet = package.Workbook.Worksheets.Add("Динамика активности");

            dynamicsSheet.Cells[1, 1].Value = "Было";
            dynamicsSheet.Cells[1, 2].Value = "Стало";
            dynamicsSheet.Cells[1, 3].Value = "Количество клиентов";
            for (int i = 1; i <= 3; i++)
            {
                dynamicsSheet.Cells[1, i].Style.Font.Bold = true;
                dynamicsSheet.Cells[1, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                dynamicsSheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int rowDyn = 2;
            foreach (var kvp in transitions)
            {
                dynamicsSheet.Cells[rowDyn, 1].Value = kvp.Key.from;
                dynamicsSheet.Cells[rowDyn, 2].Value = kvp.Key.to;
                dynamicsSheet.Cells[rowDyn, 3].Value = kvp.Value;

                if (rowDyn % 2 == 0)
                {
                    dynamicsSheet.Cells[rowDyn, 1, rowDyn, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    dynamicsSheet.Cells[rowDyn, 1, rowDyn, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 240, 240));
                }

                rowDyn++;
            }

            dynamicsSheet.Cells[dynamicsSheet.Dimension.Address].AutoFitColumns();

            // === Финал: отправка файла ===
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ClientActivityReport.xlsx");
        }


        // Метод для экспорта в PDF
        public async Task<IActionResult> ExportToPdf(string? activityLevel)
        {
            var report = await GetClientActivityReports(activityLevel); // Используем await
            var clients = report.UserActivityReports;

            using var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Подключаем шрифт Arial для поддержки кириллицы
            var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, true);
            document.SetFont(font);
            document.Add(new Paragraph("Отчет по активности клиентов")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16)
                .SetBold()
                .SetFont(font)
                .SetMarginBottom(20));

            var table = new Table(new float[] { 1, 4, 3, 2, 3, 3 }).UseAllAvailableWidth();

            // Заголовки таблицы
            table.AddHeaderCell(new Cell().Add(new Paragraph("№").SetBold()));
            table.AddHeaderCell(new Cell().Add(new Paragraph("ФИО").SetBold()));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Дата регистрации").SetBold()));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Посещено").SetBold()));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Последнее посещение").SetBold()));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Уровень активности").SetBold()));

            int counter = 1;
            foreach (var client in clients)
            {
                table.AddCell(new Cell().Add(new Paragraph(counter.ToString())));
                var nameOrEmail = string.IsNullOrWhiteSpace(client.FullName) ? client.Email : client.FullName;
                table.AddCell(new Cell().Add(new Paragraph(nameOrEmail)));
                table.AddCell(new Cell().Add(new Paragraph(client.RegistrationDate.ToShortDateString())));
                table.AddCell(new Cell().Add(new Paragraph(client.TotalAttendances.ToString())));
                table.AddCell(new Cell().Add(new Paragraph(client.LastAttendanceDate?.ToShortDateString() ?? "-")));
                table.AddCell(new Cell().Add(new Paragraph(client.ActivityLevel)));
                counter++;
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "ClientActivityReport.pdf");
        }

    }
}
