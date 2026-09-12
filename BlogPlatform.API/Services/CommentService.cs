using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.Comment;
using BlogPlatform.API.Services.Interfaces;
using BlogPlatform.API.Models;
using BlogPlatform.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CommentResponseDto> CreateCommentAsync(
     int blogId,
     CreateCommentDto createCommentDto,
     int userId)
        {
            var blog = await _context.Blogs.FindAsync(blogId);

            if (blog == null)
            {
                throw new ApiException("Blog not found.", 404);
            }

            var comment = new Comment
            {
                Text = createCommentDto.Text,
                BlogId = blogId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var createdComment = await _context.Comments
                .Include(c => c.User)
                .FirstAsync(c => c.Id == comment.Id);

            return new CommentResponseDto
            {
                Id = createdComment.Id,
                Text = createdComment.Text,
                CreatedAt = createdComment.CreatedAt,
                AuthorName = createdComment.User.Name,
                BlogId = createdComment.BlogId
            };
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByBlogAsync(int blogId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.BlogId == blogId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(comment => new CommentResponseDto
            {
                Id = comment.Id,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                AuthorName = comment.User.Name,
                BlogId = comment.BlogId
            });
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
            {
                throw new ApiException("Comment not found.", 404);
            }

            if (comment.UserId != userId)
            {
                throw new ApiException("You are not authorized to update this comment.", 403);
            }

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

        public async Task DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                throw new ApiException("Comment not found.", 404);
            }

            if (comment.UserId != userId)
            {
                throw new ApiException("You are not authorized to delete this comment.", 403);
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}