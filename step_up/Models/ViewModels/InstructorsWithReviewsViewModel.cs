namespace step_up.Models.ViewModels
{
    public class InstructorsWithReviewsViewModel
    {
        public List<Instructors> Instructors { get; set; } = null!;
        public List<InstructorReview> Reviews { get; set; } = null!;
    }
}
