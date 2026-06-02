using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinIQ.API.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public User Patient { get; set; } = null!;

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }

        public string TimeSlot { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        public string? Notes { get; set; }

        public string Type { get; set; } = "InPerson"; // InPerson, Online

        public decimal Fee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}