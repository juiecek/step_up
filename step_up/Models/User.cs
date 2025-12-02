using Microsoft.AspNetCore.Identity;

namespace step_up.Models
{
    public class User:IdentityUser
    {

        
        public string FullName { get; set; } = string.Empty;
       
        public string? Photo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CardNumber { get; set; } =string.Empty;
        // Связь с бронированиями
        public ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public ICollection<UserSubscription> UserSubscriptions { get; set; }=new List<UserSubscription>();
        public ICollection<DanceStyleReview> DanceStyleReviews { get; set; } = new List<DanceStyleReview>();
        public ICollection<InstructorReview> InstructorReviews { get; set; } = new List<InstructorReview>();

    }
}
