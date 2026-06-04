using ClinIQ.API.Data;
using ClinIQ.API.DTOs;
using ClinIQ.API.Models;
using ClinIQ.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly CloudinaryService _cloudinary;

        public DoctorsController(AppDbContext context, AuthService authService, CloudinaryService cloudinary)
        {
            _context = context;
            _authService = authService;
            _cloudinary = cloudinary;
        }

        // GET: api/Doctors
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DoctorSearchDto search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search.Specialty))
                query = query.Where(d => d.Specialty.ToLower() == search.Specialty.ToLower());

            if (!string.IsNullOrEmpty(search.City))
                query = query.Where(d => d.City != null && d.City.ToLower().Contains(search.City.ToLower()));

            if (!string.IsNullOrEmpty(search.Name))
                query = query.Where(d => d.User.FullName.ToLower().Contains(search.Name.ToLower()));

            if (search.MaxFee.HasValue)
                query = query.Where(d => d.ConsultationFee <= search.MaxFee.Value);

            if (search.IsAvailable.HasValue)
                query = query.Where(d => d.IsAvailable == search.IsAvailable.Value);

            var total = await query.CountAsync();

            var doctors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            return Ok(new PagedResult<DoctorProfileDto>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Items = doctors
            });
        }

        // GET: api/Doctors/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var d = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (d == null) return NotFound(new { message = "Doctor not found" });

            return Ok(new DoctorProfileDto
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
            });
        }

        // POST: api/Doctors (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateDoctorDto dto)
        {
            var user = await _authService.Register(dto.FullName, dto.Email, dto.Password, "Doctor");
            if (user == null)
                return BadRequest(new { message = "Email already exists" });

            user.Phone = dto.Phone;
            await _context.SaveChangesAsync();

            var doctor = new Doctor
            {
                UserId = user.Id,
                Specialty = dto.Specialty,
                SpecialtyAr = dto.SpecialtyAr,
                Bio = dto.Bio,
                BioAr = dto.BioAr,
                City = dto.City,
                CityAr = dto.CityAr,
                Area = dto.Area,
                LicenseNumber = dto.LicenseNumber,
                University = dto.University,
                YearsOfExperience = dto.YearsOfExperience,
                ConsultationFee = dto.ConsultationFee
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor created successfully", doctorId = doctor.Id });
        }

        // PUT: api/Doctors/5 (Doctor or Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Update(int id, UpdateDoctorDto dto)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return NotFound(new { message = "Doctor not found" });

            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == Roles.Doctor)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (doctor.UserId != userId)
                    return Forbid();
            }

            doctor.Bio = dto.Bio;
            doctor.BioAr = dto.BioAr;
            doctor.City = dto.City;
            doctor.CityAr = dto.CityAr;
            doctor.Area = dto.Area;
            doctor.University = dto.University;
            doctor.YearsOfExperience = dto.YearsOfExperience;
            doctor.ConsultationFee = dto.ConsultationFee;
            doctor.IsAvailable = dto.IsAvailable;
            doctor.User.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Doctor updated successfully" });
        }

        // POST: api/Doctors/{id}/upload-image (Doctor or Admin)
        [HttpPost("{id}/upload-image")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == Roles.Doctor)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (doctor.UserId != userId)
                    return Forbid();
            }

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { message = "Only JPEG, PNG, and WebP images are allowed" });

            const long maxSize = 5 * 1024 * 1024;
            if (file.Length > maxSize)
                return BadRequest(new { message = "File size must be less than 5MB" });

            // Delete old image from Cloudinary
            if (!string.IsNullOrEmpty(doctor.User.ProfileImage))
                await _cloudinary.DeleteImageAsync(doctor.User.ProfileImage);

            // Upload to Cloudinary
            var imageUrl = await _cloudinary.UploadImageAsync(file, "cliniq/doctors");
            doctor.User.ProfileImage = imageUrl;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Image uploaded successfully", imageUrl });
        }

        // DELETE: api/Doctors/5 (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound(new { message = "Doctor not found" });

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Doctor deleted successfully" });
        }
    }
}
