using BlogPlatform.API.DTO.Admin;
using BlogPlatform.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();

            return Ok(users);
        }
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(
            int id,
            UpdateUserRoleDto updateUserRoleDto)
            {
            var user = await _adminService.UpdateUserRoleAsync(
                id,
                updateUserRoleDto);

            return Ok(user);
        }

        [HttpPut("users/{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _adminService.DeactivateUserAsync(id);

            return Ok(user);
        }

        [HttpDelete("blogs/{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            await _adminService.DeleteBlogAsync(id);

            return Ok(new
            {
                message = "Blog deleted successfully by admin."
            });
        }

        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            await _adminService.DeleteCommentAsync(id);

            return Ok(new
            {
                message = "Comment deleted successfully by admin."
            });
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _adminService.GetDashboardAsync();

            return Ok(dashboard);
        }
    }
}