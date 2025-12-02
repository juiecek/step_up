namespace step_up.Models
{
    public class ScheduleDanceStyle
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; } // Внешний ключ к Schedules
        public Schedules Schedule { get; set; } = null!; // Навигационное свойство

        public int DanceStyleId { get; set; } // Внешний ключ к DanceStyles
        public DanceStyle DanceStyle { get; set; } = null!;// Навигационное свойство
        public int LevelId { get; set; }
        public Level Level { get; set; } = null!;

    }
}