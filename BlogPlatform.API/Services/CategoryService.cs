using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.Category;
using BlogPlatform.API.Services.Interfaces;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Name == createCategoryDto.Name);

            if (categoryExists)
            {
                throw new ApiException("Category already exists.", 400);
            }

            var category = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(category => new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            });
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return null;
            }

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, CreateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                throw new ApiException("Category not found.", 404);
            }

            bool exists = await _context.Categories.AnyAsync(c =>
                c.Id != id &&
                c.Name == updateCategoryDto.Name);

            if (exists)
            {
                throw new ApiException("Category already exists.", 400);
            }

            category.Name = updateCategoryDto.Name;
            category.Description = updateCategoryDto.Description;

            await _context.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<IEnumerable<BlogResponseDto>> GetBlogsByCategoryAsync(int categoryId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                throw new ApiException("Category not found.", 404);
            }

            var blogs = await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Category)
                .Where(b => b.CategoryId == categoryId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return blogs.Select(blog => new BlogResponseDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                UserId = blog.UserId,
                AuthorName = blog.User.Name,
                CategoryId = blog.CategoryId,
                CategoryName = blog.Category.Name
            });
        }


        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Blogs)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                throw new ApiException("Category not found.", 404);
            }

            if (category.Blogs.Any())
            {
                throw new ApiException(
                    "Cannot delete a category that contains blogs.",
                    400);
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();
        }
    }
}