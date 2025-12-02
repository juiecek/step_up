namespace step_up.Models
{

    public class DanceStyle
    {
        public int Id { get; set; }
        public string Name { get; set; }=string.Empty;
      
        public string Description { get; set; } = string.Empty;

        // Навигационные свойства
        public ICollection<ScheduleDanceStyle> ScheduleDanceStyles { get; set; } = new List<ScheduleDanceStyle>(); // Связь с Schedules
        public ICollection<Instructors> Instructors { get; set; } = new List<Instructors>();
        public ICollection<DanceStyleReview> DanceStyleReviews { get; set; } = new List<DanceStyleReview>();// Связь с Instructors
    }
}
