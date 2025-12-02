namespace step_up.Models
{
    public class InstructorReview
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!; // Внешний ключ на User
        public User User { get; set; } = null!; // Навигационное свойство для User

        public int InstructorId { get; set; } // Внешний ключ на Instructors
        public Instructors Instructor { get; set; } = null!; // Навигационное свойство для Instructors

        public int Rating { get; set; } // Оценка от 1 до 5
        public string Comment { get; set; } = string.Empty; // Текст отзыва
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Дата создания отзыва
    }
}
