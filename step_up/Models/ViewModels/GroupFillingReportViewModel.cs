namespace step_up.Models.ViewModels
{
    public class GroupFillingReportViewModel
    {
        public List<GroupFillingReportItem> Groups { get; set; } = new();
        public List<DanceStyle> DanceStyles { get; set; } = new();
        public List<Level> Levels { get; set; } = new();
        public List<Instructors> Instructors { get; set; } = new(); // Добавлено

        public int? SelectedDanceStyleId { get; set; }
        public int? SelectedLevelId { get; set; }
        public int? SelectedInstructorId { get; set; } // Добавлено

        public DateTime? DateFrom { get; set; } // Добавлено
        public DateTime? DateTo { get; set; } // Добавлено
    }


}
