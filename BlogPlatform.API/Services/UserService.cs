using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.User;
using BlogPlatform.API.Services.Interfaces;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDto> GetCurrentUserAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ApiException("User not found.", 404);
            }

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserResponseDto> UpdateProfileAsync(
    int userId,
    UpdateProfileDto updateProfileDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ApiException("User not found.", 404);
            }

            bool emailExists = await _context.Users.AnyAsync(u =>
                u.Email == updateProfileDto.Email &&
                u.Id != userId);

            if (emailExists)
            {
                throw new ApiException("Email is already in use.", 400);
            }

            user.Name = updateProfileDto.Username;
            user.Email = updateProfileDto.Email;

            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task ChangePasswordAsync(
    int userId,
    ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ApiException("User not found.", 404);
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                changePasswordDto.CurrentPassword,
                user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new ApiException("Current password is incorrect.", 400);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                changePasswordDto.NewPassword);

            await _context.SaveChangesAsync();
        }
    }
}