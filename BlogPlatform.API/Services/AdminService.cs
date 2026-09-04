using BlogPlatform.API.Data;
using BlogPlatform.API.DTO.Admin;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdminUserResponseDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .OrderBy(u => u.Id)
                .ToListAsync();

            return users.Select(user => new AdminUserResponseDto
            {
                Id = user.Id,
                Username = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        public async Task<AdminUserResponseDto> UpdateUserRoleAsync(
    int userId,
    UpdateUserRoleDto updateUserRoleDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ApiException("User not found.", 404);
            }

            var role = updateUserRoleDto.Role.Trim();

            if (!role.Equals("User", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(
                    "Invalid role. Allowed roles are User and Admin.",
                    400);
            }

            user.Role = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "User";

            await _context.SaveChangesAsync();
            return new AdminUserResponseDto
            {
                Id = user.Id,
                Username = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

        }
        public async Task<AdminUserResponseDto> DeactivateUserAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ApiException("User not found.", 404);
            }

            if (!user.IsActive)
            {
                throw new ApiException("User is already deactivated.", 400);
            }

            user.IsActive = false;

            await _context.SaveChangesAsync();

            return new AdminUserResponseDto
            {
                Id = user.Id,
                Username = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task DeleteBlogAsync(int blogId)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == blogId);

            if (blog == null)
            {
                throw new ApiException("Blog not found.", 404);
            }

            _context.Blogs.Remove(blog);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                throw new ApiException("Comment not found.", 404);
            }

            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();
        }

        public async Task<AdminDashboardResponseDto> GetDashboardAsync()
        {
            var totalUsers = await _context.Users.CountAsync();

            var activeUsers = await _context.Users
                .CountAsync(u => u.IsActive);

            var deactivatedUsers = await _context.Users
                .CountAsync(u => !u.IsActive);

            var totalBlogs = await _context.Blogs.CountAsync();

            var totalComments = await _context.Comments.CountAsync();

            var totalCategories = await _context.Categories.CountAsync();

            var totalLikes = await _context.BlogLikes.CountAsync();

            return new AdminDashboardResponseDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                DeactivatedUsers = deactivatedUsers,
                TotalBlogs = totalBlogs,
                TotalComments = totalComments,
                TotalCategories = totalCategories,
                TotalLikes = totalLikes
            };
        }

        public async Task<AdminUserResponseDto> ReactivateUserAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ApiException("User not found.", 404);
            }

            if (user.IsActive)
            {
                throw new ApiException("User is already active.", 400);
            }

            user.IsActive = true;

            await _context.SaveChangesAsync();

            return new AdminUserResponseDto
            {
                Id = user.Id,
                Username = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}