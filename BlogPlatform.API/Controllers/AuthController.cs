using BlogPlatform.API.DTO.Auth;
using BlogPlatform.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            return Ok(result);
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(
            RefreshTokenRequestDto refreshTokenRequestDto)
        {
            var result = await _authService.RefreshTokenAsync(
                refreshTokenRequestDto);

            return Ok(result);
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
    RefreshTokenRequestDto refreshTokenRequestDto)
        {
            await _authService.LogoutAsync(
                refreshTokenRequestDto.RefreshToken);

            return Ok(new
            {
                message = "Logged out successfully."
            });
        }
    }
}   