namespace step_up.Models
{
    public class Level
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<ScheduleDanceStyle> ScheduleDanceStyles { get; set; } = new List<ScheduleDanceStyle>();

    }
}
