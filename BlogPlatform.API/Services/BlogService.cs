using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.Blog;
using BlogPlatform.API.DTOs.Common;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Models;
using BlogPlatform.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public async Task<BlogResponseDto> UpdateBlogAsync(
     int id,
     CreateBlogDto updateBlogDto,
     int userId)
        {
            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                throw new ApiException("Blog not found.", 404);
            }

            if (blog.UserId != userId)
            {
                throw new ApiException("You are not authorized to update this blog.", 403);
            }

            blog.Title = updateBlogDto.Title;
            blog.Content = updateBlogDto.Content;
            blog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new BlogResponseDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                UserId = blog.UserId,
                AuthorName = blog.User.Name
            };
        }

        public async Task DeleteBlogAsync(int id, int userId)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                throw new ApiException("Blog not found.", 404);
            }

            if (blog.UserId != userId)
            {
                throw new ApiException("You are not authorized to delete this blog.", 403);
            }

            _context.Blogs.Remove(blog);

            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponseDto<BlogResponseDto>> GetBlogsAsync(
      PaginationDto paginationDto)
        {
            var query = _context.Blogs
                .Include(b => b.User)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(paginationDto.Search))
            {
                query = query.Where(b =>
                    b.Title.Contains(paginationDto.Search) ||
                    b.Content.Contains(paginationDto.Search));
            }

            // Filter by Author
            if (!string.IsNullOrWhiteSpace(paginationDto.AuthorName))
            {
                query = query.Where(b =>
                    b.User.Name.Contains(paginationDto.AuthorName));
            }
            // Total count before pagination
            int totalCount = await query.CountAsync();

            // Sort
            if (paginationDto.SortBy?.ToLower() == "oldest")
            {
                query = query.OrderBy(b => b.CreatedAt);
            }
            else
            {
                query = query.OrderByDescending(b => b.CreatedAt);
            }

            // Pagination
            var blogs = await query
                .Skip((paginationDto.PageNumber - 1) * paginationDto.PageSize)
                .Take(paginationDto.PageSize)
                .ToListAsync();

            var items = blogs.Select(blog => new BlogResponseDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                UserId = blog.UserId,
                AuthorName = blog.User.Name
            });

            return new PaginatedResponseDto<BlogResponseDto>
            {
                PageNumber = paginationDto.PageNumber,
                PageSize = paginationDto.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)paginationDto.PageSize),
                Items = items
            };
        }
    }
 }
