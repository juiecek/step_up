using step_up.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace step_up.ViewModels
{
    public class ClientActivityReportViewModel
    {
        // Список отчетов по активности пользователей
        public List<ClientActivityReport> UserActivityReports { get; set; } = new List<ClientActivityReport>();

        // Для фильтрации
        public string? ActivityLevel { get; set; }

        // Это для отображения в фильтре активности (например, для фильтра по уровню активности)
        public List<string> ActivityLevels { get; set; } = new List<string> { "Высокая", "Средняя", "Низкая" };
    }
}
