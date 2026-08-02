using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.Blog;
using BlogPlatform.API.Models;
using Microsoft.EntityFrameworkCore;
using BlogPlatform.API.Services.Interfaces;

namespace BlogPlatform.API.Services
{
    public class BlogService : IBlogService
    {
        private readonly AppDbContext _context;

        public BlogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BlogResponseDto> CreateBlogAsync(CreateBlogDto createBlogDto, int userId)
        {
            var blog = new Blog
            {
                Title = createBlogDto.Title,
                Content = createBlogDto.Content,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            var createdBlog = await _context.Blogs
                .Include(b => b.User)
                .FirstAsync(b => b.Id == blog.Id);

            return new BlogResponseDto
            {
                Id = createdBlog.Id,
                Title = createdBlog.Title,
                Content = createdBlog.Content,
                CreatedAt = createdBlog.CreatedAt,
                AuthorName = createdBlog.User.Name
            };
        }

        public async Task<IEnumerable<BlogResponseDto>> GetAllBlogsAsync()
        {
            var blogs = await _context.Blogs
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return blogs.Select(blog => new BlogResponseDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                CreatedAt = blog.CreatedAt,
                AuthorName = blog.User.Name
            });
        }

        public async Task<BlogResponseDto?> GetBlogByIdAsync(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return null;
            }

            return new BlogResponseDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                CreatedAt = blog.CreatedAt,
                AuthorName = blog.User.Name
            };
        }

        public Task<BlogResponseDto> UpdateBlogAsync(int id, CreateBlogDto updateBlogDto, int userId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteBlogAsync(int id, int userId)
        {
            throw new NotImplementedException();
        }

    }
}