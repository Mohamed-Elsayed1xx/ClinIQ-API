using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinIQ.API.Data;
using ClinIQ.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ClinIQ.API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        public async Task<User?> Register(string fullName, string email, string password, string role = "Patient")
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            if (exists) return null;

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;
            if (!VerifyPassword(password, user.PasswordHash)) return null;
            if (!user.IsActive) return null;
            return user;
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:ExpireDays"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken(int userId)
        {
            var old = _context.RefreshTokens.Where(r => r.UserId == userId);
            _context.RefreshTokens.RemoveRange(old);

            var token = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(30)
            };

            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return token.Token;
        }

        public async Task<User?> ValidateRefreshToken(string token)
        {
            var rt = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked && r.Expires > DateTime.UtcNow);

            return rt?.User;
        }

        public async Task<User?> LoginWithGoogle(string idToken)
        {
            try
            {
                var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Google:ClientId"] }
                };

                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        FullName = payload.Name,
                        Email = payload.Email,
                        PasswordHash = HashPassword(Guid.NewGuid().ToString()),
                        Role = "Patient",
                        ProfileImage = payload.Picture
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                return user;
            }
            catch
            {
                return null;
            }
        }
    }
}