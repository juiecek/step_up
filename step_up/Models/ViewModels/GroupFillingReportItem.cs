namespace step_up.Models.ViewModels
{
    public class GroupFillingReportItem
    {
        public string GroupName { get; set; } = string.Empty;
        public string DanceStyleName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public int CurrentCount { get; set; }
        public double FillingPercent { get; set; }
    }

}
