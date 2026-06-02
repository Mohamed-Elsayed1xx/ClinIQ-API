namespace ClinIQ.API.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Time { get; set; } = "";
        public string Status { get; set; } = "available"; // available, blocked
    }
}