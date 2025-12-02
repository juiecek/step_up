namespace step_up.Models
{
    public class DanceStyleReview
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!; // Внешний ключ на User
        public User User { get; set; } = null!; // Навигационное свойство для User

        public int ScheduleId { get; set; } // Внешний ключ на Schedules (занятие)
        public Schedules Schedule { get; set; } = null!; // Навигационное свойство для Schedules
        public int? DanceStyleId { get; set; }  // Сделай nullable, если не всегда нужно
        public DanceStyle? DanceStyle { get; set; }

        public int Rating { get; set; } // Оценка от 1 до 5
        public string Comment { get; set; } = string.Empty; // Текст отзыва
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Дата создания отзыва
    }
}
