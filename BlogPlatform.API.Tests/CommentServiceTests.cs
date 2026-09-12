using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.Comment;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Models;
using BlogPlatform.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Tests
{
    public class CommentServiceTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldCreateComment_WhenBlogExists()
        {
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "Ali",
                Email = "ali@test.com",
                PasswordHash = "hashed",
                Role = "User"
            };

            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "Test Content",
                UserId = 1,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            blog.UserId = user.Id;
            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var service = new CommentService(context);

            var dto = new CreateCommentDto
            {
                Text = "Great blog!"
            };

            var result = await service.CreateCommentAsync(
                blog.Id,
                dto,
                user.Id);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Great blog!", result.Text);
            Assert.Equal(blog.Id, result.BlogId);
            Assert.Equal("Ali", result.AuthorName);

            var comment = await context.Comments
                .FirstOrDefaultAsync(c => c.Id == result.Id);

            Assert.NotNull(comment);
            Assert.Equal("Great blog!", comment.Text);
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrowException_WhenBlogDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new CommentService(context);

            var dto = new CreateCommentDto
            {
                Text = "Test comment"
            };

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.CreateCommentAsync(
                    999,
                    dto,
                    1));

            Assert.Equal("Blog not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task GetCommentsByBlogAsync_ShouldReturnCommentsOrderedByCreatedAt()
        {
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "Ali",
                Email = "ali@test.com",
                PasswordHash = "hashed",
                Role = "User"
            };

            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "Content",
                UserId = 1,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            blog.UserId = user.Id;
            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var comment1 = new Comment
            {
                Text = "First comment",
                BlogId = blog.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var comment2 = new Comment
            {
                Text = "Second comment",
                BlogId = blog.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.AddRange(comment1, comment2);
            await context.SaveChangesAsync();

            var service = new CommentService(context);

            var result = (await service.GetCommentsByBlogAsync(
                blog.Id)).ToList();

            Assert.Equal(2, result.Count);

            Assert.Equal("First comment", result[0].Text);
            Assert.Equal("Second comment", result[1].Text);

            Assert.Equal("Ali", result[0].AuthorName);
            Assert.Equal(blog.Id, result[0].BlogId);
        }

        [Fact]
        public async Task GetCommentsByBlogAsync_ShouldReturnEmptyList_WhenBlogHasNoComments()
        {
            using var context = CreateDbContext();

            var result = (await new CommentService(context)
                .GetCommentsByBlogAsync(999))
                .ToList();

            Assert.Empty(result);
        }
        [Fact]
        public async Task UpdateCommentAsync_ShouldUpdateComment_WhenUserIsOwner()
        {
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "Ali",
                Email = "ali@test.com",
                PasswordHash = "hashed",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "Content",
                UserId = user.Id,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                Text = "Original text",
                BlogId = blog.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var service = new CommentService(context);

            var dto = new CreateCommentDto
            {
                Text = "Updated text"
            };

            var result = await service.UpdateCommentAsync(
                comment.Id,
                dto,
                user.Id);

            Assert.Equal("Updated text", result.Text);
            Assert.Equal(comment.Id, result.Id);
            Assert.Equal("Ali", result.AuthorName);

            var updatedComment = await context.Comments
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            Assert.NotNull(updatedComment);
            Assert.Equal("Updated text", updatedComment.Text);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrowException_WhenCommentDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new CommentService(context);

            var dto = new CreateCommentDto
            {
                Text = "Updated text"
            };

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.UpdateCommentAsync(999, dto, 1));

            Assert.Equal("Comment not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrowException_WhenUserIsNotOwner()
        {
            using var context = CreateDbContext();

            var owner = new User
            {
                Name = "Ali",
                Email = "ali@test.com",
                PasswordHash = "hashed",
                Role = "User"
            };

            var otherUser = new User
            {
                Name = "Sara",
                Email = "sara@test.com",
                PasswordHash = "hashed",
                Role = "User"
            };

            context.Users.AddRange(owner, otherUser);
            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "Content",
                UserId = owner.Id,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                Text = "Original text",
                BlogId = blog.Id,
                UserId = owner.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var service = new CommentService(context);

            var dto = new CreateCommentDto
            {
                Text = "Hacked text"
            };

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.UpdateCommentAsync(comment.Id, dto, otherUser.Id));

            Assert.Equal(
                "You are not authorized to update this comment.",
                exception.Message);

            Assert.Equal(403, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldDeleteComment_WhenUserIsOwner()
        {
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "Ali",
                Email = "ali@test.com",
                PasswordHash = "hashed",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "Content",
                UserId = user.Id,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                Text = "To be deleted",
                BlogId = blog.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var service = new CommentService(context);

            await service.DeleteCommentAsync(comment.Id, user.Id);

            var deletedComment = await context.Comments
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            Assert.Null(deletedComment);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldThrowException_WhenCommentDoesNotExist()
        {
            using var context = CreateDbContext();

            var service = new CommentService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.DeleteCommentAsync(999, 1));

            Assert.Equal("Comment not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldThrowException_WhenUserIsNotOwner()
        {
            using var context = CreateDbContext();

            var owner = new User
            {
                Name = "Ali",
                Email = "ali@test.com",
                PasswordHash = "hashed",
                Role = "User"
            };

            var otherUser = new User
            {
                Name = "Sara",
                Email = "sara@test.com",
                PasswordHash = "hashed",
                Role = "User"
            };

            context.Users.AddRange(owner, otherUser);
            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "Content",
                UserId = owner.Id,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                Text = "Protected comment",
                BlogId = blog.Id,
                UserId = owner.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var service = new CommentService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.DeleteCommentAsync(comment.Id, otherUser.Id));

            Assert.Equal(
                "You are not authorized to delete this comment.",
                exception.Message);

            Assert.Equal(403, exception.StatusCode);
        }
    }
}