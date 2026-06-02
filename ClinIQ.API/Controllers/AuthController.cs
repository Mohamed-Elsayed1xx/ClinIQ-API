using ClinIQ.API.DTOs;
using ClinIQ.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
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
    }
}