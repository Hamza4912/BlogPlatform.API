using BlogPlatform.API.DTOs.User;

namespace BlogPlatform.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> GetCurrentUserAsync(int userId);

        Task<UserResponseDto> UpdateProfileAsync(
            int userId,
            UpdateProfileDto updateProfileDto);

        Task ChangePasswordAsync(
            int userId,
            ChangePasswordDto changePasswordDto);
    }
}