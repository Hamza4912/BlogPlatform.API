using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.BlogLike;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Models;
using BlogPlatform.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace BlogPlatform.API.Services
{
    public class BlogLikeService : IBlogLikeService
    {
        private readonly AppDbContext _context;

        public BlogLikeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LikeBlogAsync(int blogId, int userId)
        {
            var blogExists = await _context.Blogs
                .AnyAsync(b => b.Id == blogId);

            if (!blogExists)
            {
                throw new ApiException("Blog not found.", 404);
            }

            var alreadyLiked = await _context.BlogLikes
                .AnyAsync(bl => bl.BlogId == blogId && bl.UserId == userId);

            if (alreadyLiked)
            {
                throw new ApiException("You have already liked this blog.", 400);
            }

            var like = new BlogLike
            {
                BlogId = blogId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.BlogLikes.Add(like);

            await _context.SaveChangesAsync();
        }

        public async Task UnlikeBlogAsync(int blogId, int userId)
        {
            var like = await _context.BlogLikes
                .FirstOrDefaultAsync(bl =>
                    bl.BlogId == blogId &&
                    bl.UserId == userId);

            if (like == null)
            {
                throw new ApiException("You have not liked this blog.", 400);
            }

            _context.BlogLikes.Remove(like);

            await _context.SaveChangesAsync();
        }

        public Task<LikeResponseDto> GetLikesAsync(int blogId)
        {
            throw new NotImplementedException();
        }
    }
}