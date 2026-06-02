using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinIQ.API.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string Specialty { get; set; } = string.Empty;

        public string? SpecialtyAr { get; set; }

        public string? Bio { get; set; }

        public string? BioAr { get; set; }

        public string? City { get; set; }

        public string? CityAr { get; set; }

        public string? Area { get; set; }

        public string? LicenseNumber { get; set; }

        public string? University { get; set; }

        public int YearsOfExperience { get; set; }

        public decimal ConsultationFee { get; set; }

        public double Rating { get; set; } = 0;

        public int ReviewsCount { get; set; } = 0;

        public bool IsVerified { get; set; } = false;

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}