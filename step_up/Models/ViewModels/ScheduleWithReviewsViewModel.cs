namespace step_up.Models.ViewModels
{
    public class ScheduleWithReviewsViewModel
    {
        public List<Schedules> Schedules { get; set; }
        public List<DanceStyleReview> Reviews { get; set; }
        public DanceStyleReview NewReview { get; set; } // для формы добавления
        public int DanceStyleId { get; set; }
    }

}
