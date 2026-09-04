using BlogPlatform.API.DTO.Admin;

namespace BlogPlatform.API.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminUserResponseDto>> GetAllUsersAsync();

        Task<AdminUserResponseDto> UpdateUserRoleAsync(
            int userId,
            UpdateUserRoleDto updateUserRoleDto);

        Task<AdminUserResponseDto> DeactivateUserAsync(int userId);

        Task DeleteBlogAsync(int blogId);

        Task DeleteCommentAsync(int commentId);

        Task<AdminDashboardResponseDto> GetDashboardAsync();
    }


}