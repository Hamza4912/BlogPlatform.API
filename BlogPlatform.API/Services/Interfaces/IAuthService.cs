using BlogPlatform.API.DTO.Auth;

namespace BlogPlatform.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }
}