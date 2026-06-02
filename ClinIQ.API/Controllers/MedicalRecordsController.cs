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
    [Authorize]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicalRecordsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/MedicalRecords/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRecords()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var records = await _context.MedicalRecords
                .Where(r => r.PatientId == userId)
                .Include(r => r.Doctor).ThenInclude(d => d!.User)
                .OrderByDescending(r => r.Date)
                .Select(r => new
                {
                    r.Id,
                    r.Type,
                    r.Title,
                    r.Notes,
                    r.Date,
                    DoctorName = r.Doctor != null ? r.Doctor.User.FullName : null,
                })
                .ToListAsync();

            return Ok(records);
        }

        // POST /api/MedicalRecords
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDto dto)
        {
            var record = new MedicalRecord
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                Type = dto.Type,
                Title = dto.Title,
                Notes = dto.Notes,
                Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
            };

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Record added", id = record.Id });
        }

        // DELETE /api/MedicalRecords/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var record = await _context.MedicalRecords.FindAsync(id);

            if (record == null) return NotFound();
            if (record.PatientId != userId) return Forbid();

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted" });
        }
    }

    public class CreateMedicalRecordDto
    {
        public int PatientId { get; set; }
        public int? DoctorId { get; set; }
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Notes { get; set; }
        public DateTime Date { get; set; }
    }
}