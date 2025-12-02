using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace step_up.Models
{

    public class Instructors
    {
        public int Id { get; set; } // Первичный ключ

        [Required]
        public string FullName { get; set; } = string.Empty; // Полное имя
        [Required]
        public string Description { get; set; } = string.Empty; // Описание
        
        public string Photo { get; set; } = string.Empty; // Фото
        [Required]
        public string? Phone { get; set; } // Телефон
       
        [Required(ErrorMessage = "Выберите стиль танца")]
        public int? DanceStyleId { get; set; } // 👈 СТАЛО nullable

        public DanceStyle? DanceStyle { get; set; }
        // Навигационные свойства
        public ICollection<Schedules> Schedules { get; set; } = new List<Schedules>();
        public ICollection<InstructorReview> InstructorReviews { get; set; } = new List<InstructorReview>();
    }
}