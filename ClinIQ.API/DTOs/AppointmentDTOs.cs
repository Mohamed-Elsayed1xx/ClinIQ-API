namespace ClinIQ.API.DTOs
{
    public class CreateAppointmentDto
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Type { get; set; } = "InPerson";
        public string? Notes { get; set; }
    }

    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal Fee { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateAppointmentStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}