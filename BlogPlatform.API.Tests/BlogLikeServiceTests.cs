using BlogPlatform.API.Data;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Models;
using BlogPlatform.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Tests
{
    public class BlogLikeServiceTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private async Task<(User user, Blog blog)> CreateUserAndBlog(
            AppDbContext context)
        {
            var user = new User
            {
                Name = "Ali",
                Email = "ali@test.com",
                PasswordHash = "hashed",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

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

            return (user, blog);
        }

        [Fact]
        public async Task LikeBlogAsync_ShouldCreateLike_WhenBlogExists()
        {
            using var context = CreateDbContext();

            var (user, blog) = await CreateUserAndBlog(context);

            var service = new BlogLikeService(context);

            await service.LikeBlogAsync(blog.Id, user.Id);

            var like = await context.BlogLikes
                .FirstOrDefaultAsync(bl =>
                    bl.BlogId == blog.Id &&
                    bl.UserId == user.Id);

            Assert.NotNull(like);
            Assert.Equal(blog.Id, like.BlogId);
            Assert.Equal(user.Id, like.UserId);
        }

        [Fact]
        public async Task LikeBlogAsync_ShouldThrowException_WhenBlogDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new BlogLikeService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.LikeBlogAsync(999, 1));

            Assert.Equal("Blog not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task LikeBlogAsync_ShouldThrowException_WhenBlogIsAlreadyLiked()
        {
            using var context = CreateDbContext();

            var (user, blog) = await CreateUserAndBlog(context);

            context.BlogLikes.Add(new BlogLike
            {
                BlogId = blog.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var service = new BlogLikeService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.LikeBlogAsync(blog.Id, user.Id));

            Assert.Equal(
                "You have already liked this blog.",
                exception.Message);

            Assert.Equal(400, exception.StatusCode);
        }

        [Fact]
        public async Task UnlikeBlogAsync_ShouldRemoveLike_WhenLikeExists()
        {
            using var context = CreateDbContext();

            var (user, blog) = await CreateUserAndBlog(context);

            var like = new BlogLike
            {
                BlogId = blog.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.BlogLikes.Add(like);
            await context.SaveChangesAsync();

            var service = new BlogLikeService(context);

            await service.UnlikeBlogAsync(blog.Id, user.Id);

            var deletedLike = await context.BlogLikes
                .FirstOrDefaultAsync(bl =>
                    bl.BlogId == blog.Id &&
                    bl.UserId == user.Id);

            Assert.Null(deletedLike);
        }

        [Fact]
        public async Task UnlikeBlogAsync_ShouldThrowException_WhenLikeDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new BlogLikeService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.UnlikeBlogAsync(1, 1));

            Assert.Equal(
                "You have not liked this blog.",
                exception.Message);

            Assert.Equal(400, exception.StatusCode);
        }

        [Fact]
        public async Task GetLikesAsync_ShouldReturnCorrectCount_WhenBlogHasLikes()
        {
            using var context = CreateDbContext();

            var (user, blog) = await CreateUserAndBlog(context);

            var secondUser = new User
            {
                Name = "Sara",
                Email = "sara@test.com",
                PasswordHash = "hashed",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(secondUser);
            await context.SaveChangesAsync();

            context.BlogLikes.AddRange(
                new BlogLike { BlogId = blog.Id, UserId = user.Id, CreatedAt = DateTime.UtcNow },
                new BlogLike { BlogId = blog.Id, UserId = secondUser.Id, CreatedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            var service = new BlogLikeService(context);

            var result = await service.GetLikesAsync(blog.Id);

            Assert.Equal(blog.Id, result.BlogId);
            Assert.Equal(2, result.Likes);
        }

        [Fact]
        public async Task GetLikesAsync_ShouldReturnZero_WhenBlogHasNoLikes()
        {
            using var context = CreateDbContext();

            var (user, blog) = await CreateUserAndBlog(context);

            var service = new BlogLikeService(context);

            var result = await service.GetLikesAsync(blog.Id);

            Assert.Equal(blog.Id, result.BlogId);
            Assert.Equal(0, result.Likes);
        }

        [Fact]
        public async Task GetLikesAsync_ShouldThrowException_WhenBlogDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new BlogLikeService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.GetLikesAsync(999));

            Assert.Equal("Blog not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }
    }
}