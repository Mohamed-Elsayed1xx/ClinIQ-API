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
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/Reviews/doctor/{doctorId} — كل reviews الدكتور
        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorReviews(int doctorId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.DoctorId == doctorId)
                .Include(r => r.Patient)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    PatientName = r.Patient.FullName,
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // POST /api/Reviews — إضافة review جديد
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewDto dto)
        {
            var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // تأكد إن الباشن عنده appointment completed مع الدكتور ده
            var hasCompleted = await _context.Appointments.AnyAsync(a =>
                a.PatientId == patientId &&
                a.DoctorId == dto.DoctorId &&
                a.Status == "Completed");

            if (!hasCompleted)
                return BadRequest(new { message = "You can only review doctors after a completed appointment" });

            // تأكد إنه مش عامل review قبل كده
            var alreadyReviewed = await _context.Reviews.AnyAsync(r =>
                r.PatientId == patientId && r.DoctorId == dto.DoctorId);

            if (alreadyReviewed)
                return BadRequest(new { message = "You have already reviewed this doctor" });

            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5" });

            var review = new Review
            {
                DoctorId = dto.DoctorId,
                PatientId = patientId,
                Rating = dto.Rating,
                Comment = dto.Comment,
            };

            _context.Reviews.Add(review);

            // update doctor rating تلقائياً
            var doctor = await _context.Doctors.FindAsync(dto.DoctorId);
            if (doctor != null)
            {
                var allRatings = await _context.Reviews
                    .Where(r => r.DoctorId == dto.DoctorId)
                    .Select(r => r.Rating)
                    .ToListAsync();

                allRatings.Add(dto.Rating);
                doctor.Rating = allRatings.Average();
                doctor.ReviewsCount = allRatings.Count;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Review added successfully" });
        }
    }

    public class AddReviewDto
    {
        public int DoctorId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
    }
}