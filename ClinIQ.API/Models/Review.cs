namespace ClinIQ.API.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public int PatientId { get; set; }
        public User Patient { get; set; } = null!;
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}