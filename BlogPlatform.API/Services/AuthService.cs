using BlogPlatform.API.Data;
using BlogPlatform.API.DTO.Auth;
using BlogPlatform.API.Models;
using BlogPlatform.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using BlogPlatform.API.Exceptions;
using System.Text;

namespace BlogPlatform.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == registerDto.Email);

            if (emailExists)
            {

                throw new ApiException("Email is already registered.", 400);
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var user = new User
            {
                Name = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = hashedPassword,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            string token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                throw new ApiException("Invalid email or password.", 401);
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                   loginDto.Password,
                   user.PasswordHash
               );
 
            if (!isPasswordValid)
            {
                throw new ApiException("Invalid email or password.", 401);
            }

            string token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
             {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
             };

            var key = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}