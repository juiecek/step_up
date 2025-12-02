using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace step_up.Models
{
    public class Schedules
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Выберите зал")]
        public int HallId { get; set; }

        [Required(ErrorMessage = "Выберите инструктора")]
        public int InstructorId { get; set; }

        // ❗ Навигационные свойства НЕ требуют атрибутов
        public Halls? Hall { get; set; }
        public Instructors? Instructor { get; set; }


        [Required(ErrorMessage = "Выберите день недели")]
        public DayOfWeek DayOfWeek { get; set; } // ❗ День недели (вместо конкретной даты)

        [Required(ErrorMessage = "Укажите время начала")]
        public TimeSpan StartTime { get; set; } // ❗ Время начала занятия

        [Required]
        public int Duration { get; set; }

        [Required]
        public int MaxParticipants { get; set; }

        // Навигационные свойства
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public ICollection<ScheduleDanceStyle> ScheduleDanceStyles { get; set; } = new List<ScheduleDanceStyle>();
        public ICollection<DanceStyleReview> DanceStyleReviews { get; set; } = new List<DanceStyleReview>();
    }
}

