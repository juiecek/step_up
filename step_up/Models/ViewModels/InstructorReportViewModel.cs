namespace step_up.Models.ViewModels
{
    public class InstructorReportViewModel
    {
        public Instructors Instructor { get; set; } = null!;

        public int TotalSchedules { get; set; }
        public int TotalRegistrations { get; set; }

        public double AverageRating { get; set; }
        public List<InstructorReview> Reviews { get; set; } = new();

        public List<Schedules> SchedulesInPeriod { get; set; } = new();

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string MostRecentComment => Reviews.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.Comment ?? "Нет отзывов";
    }

}
