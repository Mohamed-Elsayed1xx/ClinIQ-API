using ClinIQ.API.Data;
using ClinIQ.API.DTOs;
using ClinIQ.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly CloudinaryService _cloudinary;
        private readonly AppDbContext _context;

        public AuthController(AuthService authService, CloudinaryService cloudinary, AppDbContext context)
        {
            _authService = authService;
            _cloudinary = cloudinary;
            _context = context;
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = await _authService.Register(dto.FullName, dto.Email, dto.Password, dto.Role);
            if (user == null)
                return BadRequest(new { message = "Email already exists" });

            var token = _authService.GenerateToken(user);
            var refreshToken = await _authService.GenerateRefreshToken(user.Id);

            return Ok(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                UserId = user.Id
            });
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _authService.Login(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var token = _authService.GenerateToken(user);
            var refreshToken = await _authService.GenerateRefreshToken(user.Id);

            return Ok(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                UserId = user.Id
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenDto dto)
        {
            var user = await _authService.ValidateRefreshToken(dto.RefreshToken);
            if (user == null)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            var token = _authService.GenerateToken(user);
            var refreshToken = await _authService.GenerateRefreshToken(user.Id);

            return Ok(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                UserId = user.Id
            });
        }

        [HttpPost("google")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Google(GoogleAuthDto dto)
        {
            var user = await _authService.LoginWithGoogle(dto.IdToken);
            if (user == null)
                return Unauthorized(new { message = "Invalid Google token" });

            var token = _authService.GenerateToken(user);
            var refreshToken = await _authService.GenerateRefreshToken(user.Id);

            return Ok(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                UserId = user.Id
            });
        }

        [HttpPost("upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { message = "Only JPEG, PNG, and WebP images are allowed" });

            const long maxSize = 5 * 1024 * 1024;
            if (file.Length > maxSize)
                return BadRequest(new { message = "File size must be less than 5MB" });

            if (!string.IsNullOrEmpty(user.ProfileImage))
                await _cloudinary.DeleteImageAsync(user.ProfileImage);

            var imageUrl = await _cloudinary.UploadImageAsync(file, "cliniq/users");
            user.ProfileImage = imageUrl;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Image uploaded successfully", imageUrl });
        }
    }
}
