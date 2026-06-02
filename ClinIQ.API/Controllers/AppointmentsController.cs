using ClinIQ.API.Data;
using ClinIQ.API.DTOs;
using ClinIQ.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        // Helper — بنستخدمه في أكتر من مكان
        private async Task AddNotification(int userId, string title, string message, string type = "info")
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
            });
            await Task.CompletedTask;
        }

        // GET: api/Appointments/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d.User);

            if (role == "Patient")
                query = query.Where(a => a.PatientId == userId);
            else if (role == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return NotFound();
                query = query.Where(a => a.DoctorId == doctor.Id);
            }

            var appointments = await query.Select(a => new AppointmentResponseDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient.FullName,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                Specialty = a.Doctor.Specialty,
                AppointmentDate = a.AppointmentDate,
                TimeSlot = a.TimeSlot,
                Status = a.Status,
                Type = a.Type,
                Notes = a.Notes,
                Fee = a.Fee,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

            return Ok(appointments);
        }

        // POST: api/Appointments
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create(CreateAppointmentDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);
            if (doctor == null) return NotFound(new { message = "Doctor not found" });

            var conflict = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == dto.DoctorId &&
                a.AppointmentDate.Date == dto.AppointmentDate.Date &&
                a.TimeSlot == dto.TimeSlot &&
                a.Status != "Cancelled");

            if (conflict)
                return BadRequest(new { message = "This time slot is already booked" });

            var appointment = new Appointment
            {
                PatientId = userId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                TimeSlot = dto.TimeSlot,
                Type = dto.Type,
                Notes = dto.Notes,
                Fee = doctor.ConsultationFee,
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);

            // Notification للـ Patient
            await AddNotification(
                userId,
                "Appointment Booked",
                $"Your appointment with {doctor.User.FullName} on {dto.AppointmentDate:MMM dd} at {dto.TimeSlot} is pending confirmation.",
                "info"
            );

            // Notification للـ Doctor
            await AddNotification(
                doctor.UserId,
                "New Appointment Request",
                $"You have a new appointment request on {dto.AppointmentDate:MMM dd} at {dto.TimeSlot}.",
                "info"
            );

            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment booked successfully", appointmentId = appointment.Id });
        }

        // PUT: api/Appointments/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateAppointmentStatusDto dto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound(new { message = "Appointment not found" });

            appointment.Status = dto.Status;

            // Notification للـ Patient عند التأكيد أو الإلغاء
            if (dto.Status == "Confirmed")
            {
                await AddNotification(
                    appointment.PatientId,
                    "Appointment Confirmed ✓",
                    $"Your appointment with {appointment.Doctor.User.FullName} on {appointment.AppointmentDate:MMM dd} at {appointment.TimeSlot} has been confirmed.",
                    "success"
                );
            }
            else if (dto.Status == "Cancelled")
            {
                await AddNotification(
                    appointment.PatientId,
                    "Appointment Cancelled",
                    $"Your appointment with {appointment.Doctor.User.FullName} on {appointment.AppointmentDate:MMM dd} at {appointment.TimeSlot} has been cancelled.",
                    "warning"
                );
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully" });
        }

        // DELETE: api/Appointments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound(new { message = "Appointment not found" });
            if (appointment.PatientId != userId) return Forbid();

            appointment.Status = "Cancelled";

            await AddNotification(
                userId,
                "Appointment Cancelled",
                $"Your appointment with {appointment.Doctor.User.FullName} on {appointment.AppointmentDate:MMM dd} at {appointment.TimeSlot} has been cancelled.",
                "warning"
            );

            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment cancelled successfully" });
        }
    }
}