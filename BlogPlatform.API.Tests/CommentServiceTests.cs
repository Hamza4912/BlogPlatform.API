using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.Comment;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Models;
using BlogPlatform.API.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogPlatform.Tests.Services
{
    public class CommentServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly CommentService _sut; // System Under Test

        public CommentServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new CommentService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ---------- Helper ----------
        private async Task<User> SeedUserAsync(string name = "Ali")
        {
            var user = new User { Name = name, Email = $"{name}@test.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private async Task<Blog> SeedBlogAsync(int userId, string title = "My Blog")
        {
            var blog = new Blog
            {
                Title = title,
                Content = "Content",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();
            return blog;
        }

        // ============================================================
        //  CreateCommentAsync
        // ============================================================

        [Fact]
        public async Task CreateCommentAsync_WhenBlogExists_ReturnsCommentResponseDto()
        {
            // Arrange
            var user = await SeedUserAsync("Ali");
            var blog = await SeedBlogAsync(user.Id);
            var dto = new CreateCommentDto { Text = "Nice post!" };

            // Act
            var result = await _sut.CreateCommentAsync(blog.Id, dto, user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Nice post!", result.Text);
            Assert.Equal(blog.Id, result.BlogId);
            Assert.Equal("Ali", result.AuthorName);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task CreateCommentAsync_WhenBlogNotFound_ThrowsApiException()
        {
            // Arrange
            var user = await SeedUserAsync();
            var dto = new CreateCommentDto { Text = "Comment" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApiException>(
                () => _sut.CreateCommentAsync(999, dto, user.Id));

            Assert.Equal(404, ex.StatusCode);
            Assert.Equal("Blog not found.", ex.Message);
        }

        [Fact]
        public async Task CreateCommentAsync_SavesCommentToDatabase()
        {
            // Arrange
            var user = await SeedUserAsync();
            var blog = await SeedBlogAsync(user.Id);
            var dto = new CreateCommentDto { Text = "Hello" };

            // Act
            await _sut.CreateCommentAsync(blog.Id, dto, user.Id);

            // Assert
            var commentInDb = await _context.Comments.FirstOrDefaultAsync();
            Assert.NotNull(commentInDb);
            Assert.Equal("Hello", commentInDb.Text);
            Assert.Equal(blog.Id, commentInDb.BlogId);
            Assert.Equal(user.Id, commentInDb.UserId);
        }

        // ============================================================
        //  GetCommentsByBlogAsync
        // ============================================================

        [Fact]
        public async Task GetCommentsByBlogAsync_WhenCommentsExist_ReturnsOrderedComments()
        {
            // Arrange
            var user = await SeedUserAsync("Sara");
            var blog = await SeedBlogAsync(user.Id);

            _context.Comments.AddRange(
                new Comment { Text = "First", BlogId = blog.Id, UserId = user.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
                new Comment { Text = "Second", BlogId = blog.Id, UserId = user.Id, CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = (await _sut.GetCommentsByBlogAsync(blog.Id)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("First", result[0].Text);   // oldest first
            Assert.Equal("Second", result[1].Text);
        }

        [Fact]
        public async Task GetCommentsByBlogAsync_WhenNoComments_ReturnsEmptyList()
        {
            // Arrange
            var user = await SeedUserAsync();
            var blog = await SeedBlogAsync(user.Id);

            // Act
            var result = await _sut.GetCommentsByBlogAsync(blog.Id);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCommentsByBlogAsync_OnlyReturnsCommentsForGivenBlog()
        {
            // Arrange
            var user = await SeedUserAsync();
            var blog1 = await SeedBlogAsync(user.Id, "Blog 1");
            var blog2 = await SeedBlogAsync(user.Id, "Blog 2");

            _context.Comments.AddRange(
                new Comment { Text = "For blog1", BlogId = blog1.Id, UserId = user.Id, CreatedAt = DateTime.UtcNow },
                new Comment { Text = "For blog2", BlogId = blog2.Id, UserId = user.Id, CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = (await _sut.GetCommentsByBlogAsync(blog1.Id)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("For blog1", result[0].Text);
        }

        [Fact]
        public async Task GetCommentsByBlogAsync_ReturnsAuthorNameCorrectly()
        {
            // Arrange
            var user = await SeedUserAsync("Hassan");
            var blog = await SeedBlogAsync(user.Id);
            _context.Comments.Add(new Comment
            {
                Text = "Test",
                BlogId = blog.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act
            var result = (await _sut.GetCommentsByBlogAsync(blog.Id)).First();

            // Assert
            Assert.Equal("Hassan", result.AuthorName);
        }

        // ============================================================
        //  UpdateCommentAsync (currently NotImplementedException)
        // ============================================================

        [Fact]
        public async Task UpdateCommentAsync_CurrentlyThrowsNotImplementedException()
        {
            // Arrange
            var dto = new CreateCommentDto { Text = "Updated" };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(
                () => _sut.UpdateCommentAsync(1, dto, 1));
        }

        // ============================================================
        //  DeleteCommentAsync (currently NotImplementedException)
        // ============================================================

        [Fact]
        public async Task DeleteCommentAsync_CurrentlyThrowsNotImplementedException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(
                () => _sut.DeleteCommentAsync(1, 1));
        }
        public async Task<CommentResponseDto> UpdateCommentAsync(
    int commentId,
    CreateCommentDto updateCommentDto,
    int userId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new ApiException("Comment not found.", 404);

            if (comment.UserId != userId)
                throw new ApiException("You are not allowed to update this comment.", 403);

            comment.Text = updateCommentDto.Text;

            await _context.SaveChangesAsync();

            return new CommentResponseDto
            {
                Id = comment.Id,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                AuthorName = comment.User.Name,
                BlogId = comment.BlogId
            };
        }

        [Fact]
        public async Task UpdateCommentAsync_WhenOwnerUpdates_ReturnsUpdatedDto()
        {
            var user = await SeedUserAsync("Ali");
            var blog = await SeedBlogAsync(user.Id);
            var comment = new Comment { Text = "Old", BlogId = blog.Id, UserId = user.Id, CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var result = await _sut.UpdateCommentAsync(comment.Id, new CreateCommentDto { Text = "New" }, user.Id);

            Assert.Equal("New", result.Text);
            Assert.Equal("Ali", result.AuthorName);
        }

        [Fact]
        public async Task UpdateCommentAsync_WhenNotOwner_Throws403()
        {
            var owner = await SeedUserAsync("Ali");
            var other = await SeedUserAsync("Bob");
            var blog = await SeedBlogAsync(owner.Id);
            var comment = new Comment { Text = "Old", BlogId = blog.Id, UserId = owner.Id, CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<ApiException>(
                () => _sut.UpdateCommentAsync(comment.Id, new CreateCommentDto { Text = "Hacked" }, other.Id));

            Assert.Equal(403, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateCommentAsync_WhenCommentMissing_Throws404()
        {
            var ex = await Assert.ThrowsAsync<ApiException>(
                () => _sut.UpdateCommentAsync(999, new CreateCommentDto { Text = "X" }, 1));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenOwnerDeletes_RemovesFromDb()
        {
            var user = await SeedUserAsync();
            var blog = await SeedBlogAsync(user.Id);
            var comment = new Comment { Text = "Bye", BlogId = blog.Id, UserId = user.Id, CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            await _sut.DeleteCommentAsync(comment.Id, user.Id);

            Assert.Null(await _context.Comments.FindAsync(comment.Id));
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenNotOwner_Throws403()
        {
            var owner = await SeedUserAsync("Ali");
            var other = await SeedUserAsync("Bob");
            var blog = await SeedBlogAsync(owner.Id);
            var comment = new Comment { Text = "Stay", BlogId = blog.Id, UserId = owner.Id, CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<ApiException>(
                () => _sut.DeleteCommentAsync(comment.Id, other.Id));

            Assert.Equal(403, ex.StatusCode);
            Assert.NotNull(await _context.Comments.FindAsync(comment.Id));
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenCommentMissing_Throws404()
        {
            var ex = await Assert.ThrowsAsync<ApiException>(
                () => _sut.DeleteCommentAsync(999, 1));
            Assert.Equal(404, ex.StatusCode);
        }
    }
}