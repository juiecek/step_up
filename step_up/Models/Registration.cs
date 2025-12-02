using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace step_up.Models
{
    public class Registration
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [ForeignKey("ScheduleId")]
        public Schedules? Schedule { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }  // 📌 Новое поле — дата конкретного занятия

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Required]
        public int UserSubscriptionId { get; set; }

        [ForeignKey("UserSubscriptionId")]
        public UserSubscription? UserSubscription { get; set; }

        public bool Attended { get; set; } = false;
    }
}
