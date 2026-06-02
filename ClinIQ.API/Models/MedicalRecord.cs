namespace ClinIQ.API.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public User Patient { get; set; } = null!;
        public int? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public string Type { get; set; } = ""; // Lab Result, Prescription, Imaging, Report
        public string Title { get; set; } = "";
        public string? Notes { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}