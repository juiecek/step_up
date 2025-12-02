using System;

namespace step_up.Models
{
    public class Bookings
    {
        public int Id { get; set; } // Первичный ключ
        public string UserId { get; set; } = string.Empty; // Внешний ключ к Users
        public User User { get; set; } = null!; // Навигационное свойство
        public int HallId { get; set; } // Внешний ключ к Halls
        public Halls Hall { get; set; } = null!; // Навигационное свойство
        public DateTime BookingDate { get; set; } // Дата бронирования
        public int Duration { get; set; } // Длительность в часах
        public decimal TotalPrice { get; set; } // Общая стоимость
        public string Status { get; set; } = "Pending"; // Статус бронирования

    }
}
