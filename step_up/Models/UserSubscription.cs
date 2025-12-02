namespace step_up.Models
{
    public class UserSubscription
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public int SubscriptionId {  get; set; }
        public Subscription Subscription { get; set; } = null!;
        public DateTime PurchaseDate {  get; set; }= DateTime.Now;// дата полцчения
        public DateTime ExpiryDate {  get; set; } // сколько до окончания дней
        public int ClassesRemaining { get; set; } // количество занятий 


    }
}
