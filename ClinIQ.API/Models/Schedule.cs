using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinIQ.API.Models
{
    public class Schedule
    {
        public int Id { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public string DayOfWeek { get; set; } = string.Empty; // Monday, Tuesday...

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public int SlotDurationMinutes { get; set; } = 30;

        public bool IsAvailable { get; set; } = true;
    }
}