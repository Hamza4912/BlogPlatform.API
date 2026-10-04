using BlogPlatform.API.Data;
using BlogPlatform.API.DTO.Admin;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Models;
using BlogPlatform.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Tests
{
    public class AdminServiceTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private User CreateUser(
            string name,
            string email,
            string role = "User",
            bool isActive = true)
        {
            return new User
            {
                Name = name,
                Email = email,
                PasswordHash = "hashed",
                Role = role,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsersOrderedById()
        {
            using var context = CreateDbContext();

            var user1 = CreateUser("Ali", "ali@test.com");
            var user2 = CreateUser("Ahmed", "ahmed@test.com");

            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            var service = new AdminService(context);

            var result = (await service.GetAllUsersAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(user1.Id, result[0].Id);
            Assert.Equal(user2.Id, result[1].Id);
            Assert.Equal("Ali", result[0].Username);
            Assert.Equal("Ahmed", result[1].Username);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            using var context = CreateDbContext();

            var service = new AdminService(context);

            var result = (await service.GetAllUsersAsync()).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_ShouldChangeRoleToAdmin()
        {
            using var context = CreateDbContext();

            var user = CreateUser("Ali", "ali@test.com");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AdminService(context);

            var dto = new UpdateUserRoleDto
            {
                Role = "Admin"
            };

            var result = await service.UpdateUserRoleAsync(
                user.Id,
                dto);

            Assert.Equal("Admin", result.Role);

            var updatedUser = await context.Users
                .FirstAsync(u => u.Id == user.Id);

            Assert.Equal("Admin", updatedUser.Role);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_ShouldChangeRoleToUser()
        {
            using var context = CreateDbContext();

            var user = CreateUser(
                "Ali",
                "ali@test.com",
                "Admin");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AdminService(context);

            var dto = new UpdateUserRoleDto
            {
                Role = "User"
            };

            var result = await service.UpdateUserRoleAsync(
                user.Id,
                dto);

            Assert.Equal("User", result.Role);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new AdminService(context);

            var dto = new UpdateUserRoleDto
            {
                Role = "Admin"
            };

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.UpdateUserRoleAsync(999, dto));

            Assert.Equal("User not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_ShouldThrowException_WhenRoleIsInvalid()
        {
            using var context = CreateDbContext();

            var user = CreateUser("Ali", "ali@test.com");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AdminService(context);

            var dto = new UpdateUserRoleDto
            {
                Role = "Moderator"
            };

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.UpdateUserRoleAsync(user.Id, dto));

            Assert.Equal(
                "Invalid role. Allowed roles are User and Admin.",
                exception.Message);

            Assert.Equal(400, exception.StatusCode);
        }

        [Fact]
        public async Task DeactivateUserAsync_ShouldDeactivateActiveUser()
        {
            using var context = CreateDbContext();

            var user = CreateUser(
                "Ali",
                "ali@test.com",
                "User",
                true);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AdminService(context);

            var result = await service.DeactivateUserAsync(user.Id);

            Assert.False(result.IsActive);

            var updatedUser = await context.Users
                .FirstAsync(u => u.Id == user.Id);

            Assert.False(updatedUser.IsActive);
        }

        [Fact]
        public async Task DeactivateUserAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new AdminService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.DeactivateUserAsync(999));

            Assert.Equal("User not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task DeactivateUserAsync_ShouldThrowException_WhenUserAlreadyDeactivated()
        {
            using var context = CreateDbContext();

            var user = CreateUser(
                "Ali",
                "ali@test.com",
                "User",
                false);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AdminService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.DeactivateUserAsync(user.Id));

            Assert.Equal(
                "User is already deactivated.",
                exception.Message);

            Assert.Equal(400, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteBlogAsync_ShouldDeleteBlog_WhenBlogExists()
        {
            using var context = CreateDbContext();

            var user = CreateUser("Ali", "ali@test.com");

            var category = new Category
            {
                Name = "Technology",
                Description = "Tech"
            };

            context.Users.Add(user);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "Test Content",
                UserId = user.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var blogId = blog.Id;

            var service = new AdminService(context);

            await service.DeleteBlogAsync(blogId);

            var deletedBlog = await context.Blogs
                .FirstOrDefaultAsync(b => b.Id == blogId);

            Assert.Null(deletedBlog);
        }

        [Fact]
        public async Task DeleteBlogAsync_ShouldThrowException_WhenBlogDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new AdminService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.DeleteBlogAsync(999));

            Assert.Equal("Blog not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldDeleteComment_WhenCommentExists()
        {
            using var context = CreateDbContext();

            var user = CreateUser("Ali", "ali@test.com");

            var category = new Category
            {
                Name = "Technology",
                Description = "Tech"
            };

            context.Users.Add(user);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "Content",
                UserId = user.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                Text = "Test comment",
                BlogId = blog.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var commentId = comment.Id;

            var service = new AdminService(context);

            await service.DeleteCommentAsync(commentId);

            var deletedComment = await context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            Assert.Null(deletedComment);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldThrowException_WhenCommentDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new AdminService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.DeleteCommentAsync(999));

            Assert.Equal("Comment not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnCorrectStatistics()
        {
            using var context = CreateDbContext();

            var activeUser = CreateUser(
                "Ali",
                "ali@test.com",
                "User",
                true);

            var inactiveUser = CreateUser(
                "Ahmed",
                "ahmed@test.com",
                "User",
                false);

            context.Users.AddRange(activeUser, inactiveUser);

            var category = new Category
            {
                Name = "Technology",
                Description = "Tech"
            };

            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "Content",
                UserId = activeUser.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                Text = "Nice blog",
                BlogId = blog.Id,
                UserId = activeUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            var like = new BlogLike
            {
                BlogId = blog.Id,
                UserId = activeUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            context.BlogLikes.Add(like);

            await context.SaveChangesAsync();

            var service = new AdminService(context);

            var result = await service.GetDashboardAsync();

            Assert.Equal(2, result.TotalUsers);
            Assert.Equal(1, result.ActiveUsers);
            Assert.Equal(1, result.DeactivatedUsers);
            Assert.Equal(1, result.TotalBlogs);
            Assert.Equal(1, result.TotalComments);
            Assert.Equal(1, result.TotalCategories);
            Assert.Equal(1, result.TotalLikes);
        }
    }
}