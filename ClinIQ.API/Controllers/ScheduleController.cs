using ClinIQ.API.Data;
using ClinIQ.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScheduleController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/Schedule/{doctorId}?date=2026-05-22
        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetSlots(int doctorId, [FromQuery] string? date)
        {
            var startDate = date != null
                ? DateTime.SpecifyKind(DateTime.Parse(date), DateTimeKind.Utc)
                : DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(7);

            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate >= startDate &&
                            a.AppointmentDate < endDate &&
                            a.Status != "Cancelled")
                .Select(a => new
                {
                    date = a.AppointmentDate.Date.ToString("yyyy-MM-dd"),
                    time = a.TimeSlot
                })
                .ToListAsync();

            var blockedSlots = await _context.TimeSlots
                .Where(t => t.DoctorId == doctorId &&
                            t.Date >= startDate &&
                            t.Date < endDate &&
                            t.Status == "blocked")
                .Select(t => new
                {
                    date = t.Date.ToString("yyyy-MM-dd"),
                    time = t.Time
                })
                .ToListAsync();

            return Ok(new { bookedSlots, blockedSlots });
        }

        // PUT /api/Schedule/toggle
        [HttpPut("toggle")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ToggleSlot([FromBody] ToggleSlotDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound(new { message = "Doctor not found" });

            var date = DateTime.SpecifyKind(DateTime.Parse(dto.Date), DateTimeKind.Utc);

            var existing = await _context.TimeSlots.FirstOrDefaultAsync(t =>
                t.DoctorId == doctor.Id &&
                t.Date.Date == date.Date &&
                t.Time == dto.Time);

            if (existing == null)
            {
                _context.TimeSlots.Add(new TimeSlot
                {
                    DoctorId = doctor.Id,
                    Date = date,
                    Time = dto.Time,
                    Status = "blocked"
                });
            }
            else
            {
                _context.TimeSlots.Remove(existing);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Slot updated" });
        }
    }

    public class ToggleSlotDto
    {
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";
    }
}