namespace step_up.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Название: "Разовый", "8 занятий"
        public decimal Price { get; set; } // Цена
        public int NumberOfClasses { get; set; } // Кол-во занятий (0 — безлимит)
        public int DurationInDays { get; set; } // Срок действия в днях

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
