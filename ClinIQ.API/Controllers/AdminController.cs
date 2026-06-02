using ClinIQ.API.Data;
using ClinIQ.API.DTOs;
using ClinIQ.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Admin/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalDoctors = await _context.Doctors.CountAsync();
            var totalPatients = await _context.Users.CountAsync(u => u.Role == Roles.Patient);
            var totalAppointments = await _context.Appointments.CountAsync();
            var pendingAppointments = await _context.Appointments.CountAsync(a => a.Status == "Pending");
            var completedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Completed");
            var totalRevenue = await _context.Appointments
                .Where(a => a.Status == "Completed")
                .SumAsync(a => a.Fee);

            return Ok(new
            {
                totalUsers,
                totalDoctors,
                totalPatients,
                totalAppointments,
                pendingAppointments,
                completedAppointments,
                totalRevenue
            });
        }

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.Phone,
                    u.IsActive,
                    u.CreatedAt
                }).ToListAsync();

            return Ok(users);
        }

        // PUT: api/Admin/users/5/toggle
        [HttpPut("users/{id}/toggle")]
        public async Task<IActionResult> ToggleUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found" });

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully" });
        }

        // GET: api/Admin/appointments
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Select(a => new AppointmentResponseDto
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

        // GET: api/Admin/doctors
        [HttpGet("doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Select(d => new DoctorProfileDto
                {
                    Id = d.Id,
                    FullName = d.User.FullName,
                    Email = d.User.Email,
                    Phone = d.User.Phone,
                    ProfileImage = d.User.ProfileImage,
                    Specialty = d.Specialty,
                    SpecialtyAr = d.SpecialtyAr,
                    Bio = d.Bio,
                    BioAr = d.BioAr,
                    City = d.City,
                    CityAr = d.CityAr,
                    Area = d.Area,
                    LicenseNumber = d.LicenseNumber,
                    University = d.University,
                    YearsOfExperience = d.YearsOfExperience,
                    ConsultationFee = d.ConsultationFee,
                    Rating = d.Rating,
                    ReviewsCount = d.ReviewsCount,
                    IsVerified = d.IsVerified,
                    IsAvailable = d.IsAvailable
                }).ToListAsync();

            return Ok(doctors);
        }

        // PUT: api/Admin/doctors/5/verify
        [HttpPut("doctors/{id}/verify")]
        public async Task<IActionResult> VerifyDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound(new { message = "Doctor not found" });

            doctor.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor verified successfully" });
        }

        // GET: api/Admin/revenue
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
            var startOfMonth = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            // Total & this month — في SQL مباشرة
            var totalRevenue = await _context.Appointments
                .Where(a => a.Status == "Completed")
                .SumAsync(a => a.Fee);

            var thisMonth = await _context.Appointments
                .Where(a => a.Status == "Completed" &&
                            a.AppointmentDate.Month == DateTime.UtcNow.Month &&
                            a.AppointmentDate.Year == DateTime.UtcNow.Year)
                .SumAsync(a => a.Fee);

            // Monthly revenue — GROUP BY في SQL
            var monthlyRaw = await _context.Appointments
                .Where(a => a.Status == "Completed" &&
                            a.AppointmentDate >= startOfMonth)
                .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(a => a.Fee)
                })
                .ToListAsync();

            // بنبني الـ 6 شهور كاملة حتى لو مفيش بيانات
            var monthly = Enumerable.Range(0, 6).Select(i =>
            {
                var date = DateTime.UtcNow.AddMonths(-5 + i);
                var found = monthlyRaw.FirstOrDefault(m =>
                    m.Year == date.Year && m.Month == date.Month);
                return new
                {
                    month = date.ToString("MMM"),
                    revenue = found?.Revenue ?? 0
                };
            }).ToList();

            // Top doctors — GROUP BY في SQL
            var topDoctors = await _context.Appointments
                .Where(a => a.Status == "Completed")
                .GroupBy(a => new { a.DoctorId, a.Doctor.User.FullName, a.Doctor.Specialty })
                .Select(g => new
                {
                    doctorId = g.Key.DoctorId,
                    name = g.Key.FullName,
                    specialty = g.Key.Specialty,
                    earnings = g.Sum(a => a.Fee),
                    appointments = g.Count()
                })
                .OrderByDescending(x => x.earnings)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                totalRevenue,
                thisMonth,
                platformFee = totalRevenue * 0.15m,
                monthly,
                topDoctors
            });
        }
    }
}
