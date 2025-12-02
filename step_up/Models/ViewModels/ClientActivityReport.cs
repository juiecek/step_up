namespace step_up.Models.ViewModels
{
    public class ClientActivityReport
    {
        public string FullName { get; set; } = string.Empty;
        public string Email {  get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public int TotalAttendances { get; set; }
        public DateTime? LastAttendanceDate { get; set; }
        public string ActivityLevel { get; set; } = string.Empty;
    }
}
