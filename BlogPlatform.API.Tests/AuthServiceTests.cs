using BlogPlatform.API.Data;
using BlogPlatform.API.DTO.Auth;
using BlogPlatform.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlogPlatform.API.Tests
{
    public class AuthServiceTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private IConfiguration CreateConfiguration()
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsASecretKeyForUnitTesting123456",
                ["Jwt:Issuer"] = "BlogPlatform",
                ["Jwt:Audience"] = "BlogPlatformUsers",
                ["Jwt:DurationInMinutes"] = "30"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUserAndReturnTokens()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await authService.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);

            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));

            Assert.Equal("TestUser", result.Username);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("User", result.Role);

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "test@example.com");

            Assert.NotNull(user);

            Assert.Equal("TestUser", user.Name);

            Assert.NotEqual(
                "Password123!",
                user.PasswordHash);

            Assert.True(
                BCrypt.Net.BCrypt.Verify(
                    "Password123!",
                    user.PasswordHash));

            var refreshToken = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == user.Id);

            Assert.NotNull(refreshToken);

            Assert.Equal(
                result.RefreshToken,
                refreshToken.Token);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var firstUser = new RegisterDto
            {
                Username = "FirstUser",
                Email = "same@example.com",
                Password = "Password123!"
            };

            var secondUser = new RegisterDto
            {
                Username = "SecondUser",
                Email = "same@example.com",
                Password = "Password456!"
            };

            // Act
            await authService.RegisterAsync(firstUser);

            // Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.RegisterAsync(secondUser));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "LoginUser",
                Email = "login@example.com",
                Password = "Password123!"
            };

            await authService.RegisterAsync(registerDto);

            var loginDto = new LoginDto
            {
                Email = "login@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);

            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));

            Assert.Equal("LoginUser", result.Username);
            Assert.Equal("login@example.com", result.Email);
            Assert.Equal("User", result.Role);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenPasswordIsInvalid()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "InvalidPasswordUser",
                Email = "invalid@example.com",
                Password = "CorrectPassword123!"
            };

            await authService.RegisterAsync(registerDto);

            var loginDto = new LoginDto
            {
                Email = "invalid@example.com",
                Password = "WrongPassword123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.LoginAsync(loginDto));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenEmailDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var loginDto = new LoginDto
            {
                Email = "notfound@example.com",
                Password = "Password123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.LoginAsync(loginDto));
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "RefreshUser",
                Email = "refresh@example.com",
                Password = "Password123!"
            };

            var registerResult = await authService.RegisterAsync(registerDto);

            var request = new RefreshTokenRequestDto
            {
                RefreshToken = registerResult.RefreshToken
            };

            // Act
            var result = await authService.RefreshTokenAsync(request);

            // Assert
            Assert.NotNull(result);

            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));

            Assert.Equal("RefreshUser", result.Username);
            Assert.Equal("refresh@example.com", result.Email);
            Assert.Equal("User", result.Role);

            Assert.NotEqual(
                registerResult.RefreshToken,
                result.RefreshToken);

            var oldToken = await context.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.Token == registerResult.RefreshToken);

            Assert.NotNull(oldToken);
            Assert.NotNull(oldToken.RevokedAt);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldThrowException_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var request = new RefreshTokenRequestDto
            {
                RefreshToken = "invalid-refresh-token"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.RefreshTokenAsync(request));
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldThrowException_WhenRefreshTokenIsRevoked()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "RevokedUser",
                Email = "revoked@example.com",
                Password = "Password123!"
            };

            var registerResult = await authService.RegisterAsync(registerDto);

            // Manually revoke the refresh token
            var refreshToken = await context.RefreshTokens
                .FirstAsync(rt => rt.Token == registerResult.RefreshToken);

            refreshToken.RevokedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            var request = new RefreshTokenRequestDto
            {
                RefreshToken = registerResult.RefreshToken
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.RefreshTokenAsync(request));
        }

        [Fact]
        public async Task LogoutAsync_ShouldRevokeRefreshToken_WhenTokenIsValid()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "LogoutUser",
                Email = "logout@example.com",
                Password = "Password123!"
            };

            var registerResult = await authService.RegisterAsync(registerDto);

            // Act
            await authService.LogoutAsync(registerResult.RefreshToken);

            // Assert
            var refreshToken = await context.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.Token == registerResult.RefreshToken);

            Assert.NotNull(refreshToken);
            Assert.NotNull(refreshToken.RevokedAt);
        }
        [Fact]
        public async Task LogoutAsync_ShouldThrowException_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.LogoutAsync(
                    "invalid-refresh-token"));
        }
        [Fact]
        public async Task LogoutAsync_ShouldThrowException_WhenRefreshTokenIsAlreadyRevoked()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "AlreadyRevokedUser",
                Email = "alreadyrevoked@example.com",
                Password = "Password123!"
            };

            var registerResult = await authService.RegisterAsync(registerDto);

            // First logout → revoke token
            await authService.LogoutAsync(registerResult.RefreshToken);

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.LogoutAsync(
                    registerResult.RefreshToken));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenUserIsDeactivated()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "DeactivatedUser",
                Email = "deactivated@example.com",
                Password = "Password123!"
            };

            await authService.RegisterAsync(registerDto);

            var user = await context.Users
                .FirstAsync(u => u.Email == "deactivated@example.com");

            user.IsActive = false;

            await context.SaveChangesAsync();

            var loginDto = new LoginDto
            {
                Email = "deactivated@example.com",
                Password = "Password123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.LoginAsync(loginDto));
        }


        [Fact]
        public async Task RefreshTokenAsync_ShouldThrowException_WhenUserIsDeactivated()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "DeactivatedRefreshUser",
                Email = "deactivatedrefresh@example.com",
                Password = "Password123!"
            };

            var registerResult = await authService.RegisterAsync(registerDto);

            var user = await context.Users
                .FirstAsync(u => u.Email == "deactivatedrefresh@example.com");

            user.IsActive = false;

            await context.SaveChangesAsync();

            var request = new RefreshTokenRequestDto
            {
                RefreshToken = registerResult.RefreshToken
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.RefreshTokenAsync(request));
        }
        [Fact]
        public async Task RefreshTokenAsync_ShouldThrowException_WhenRefreshTokenIsExpired()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "ExpiredTokenUser",
                Email = "expired@example.com",
                Password = "Password123!"
            };

            var registerResult = await authService.RegisterAsync(registerDto);

            var refreshToken = await context.RefreshTokens
                .FirstAsync(rt => rt.Token == registerResult.RefreshToken);

            refreshToken.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

            await context.SaveChangesAsync();

            var request = new RefreshTokenRequestDto
            {
                RefreshToken = registerResult.RefreshToken
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.RefreshTokenAsync(request));
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldThrowException_WhenTokenWasRevokedByLogout()
        {
            // Arrange
            using var context = CreateDbContext();
            var configuration = CreateConfiguration();

            var authService = new AuthService(
                context,
                configuration);

            var registerDto = new RegisterDto
            {
                Username = "LogoutRefreshUser",
                Email = "logoutrefresh@example.com",
                Password = "Password123!"
            };

            var registerResult = await authService.RegisterAsync(registerDto);

            // Logout → revoke refresh token
            await authService.LogoutAsync(registerResult.RefreshToken);

            var request = new RefreshTokenRequestDto
            {
                RefreshToken = registerResult.RefreshToken
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await authService.RefreshTokenAsync(request));
        }
    }
}