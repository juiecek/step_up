namespace step_up.Models
{
    public class Halls
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Photo { get; set; }
        public decimal Price { get; set; }

        // Связь с бронированиями и расписанием
        public ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();
        public ICollection<Schedules> Schedules { get; set; } = new List<Schedules>();
    }
}
