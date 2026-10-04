using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.Category;
using BlogPlatform.API.DTOs.Blog;
using BlogPlatform.API.Exceptions;
using BlogPlatform.API.Models;
using BlogPlatform.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Tests
{
    public class CategoryServiceTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCreateCategory_WhenDataIsValid()
        {
            using var context = CreateDbContext();
            var service = new CategoryService(context);

            var dto = new CreateCategoryDto
            {
                Name = "Technology",
                Description = "Technology related blogs"
            };

            var result = await service.CreateCategoryAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Technology", result.Name);
            Assert.Equal("Technology related blogs", result.Description);

            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == result.Id);

            Assert.NotNull(category);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowException_WhenCategoryAlreadyExists()
        {
            using var context = CreateDbContext();

            context.Categories.Add(new Category
            {
                Name = "Technology",
                Description = "Existing category"
            });

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var dto = new CreateCategoryDto
            {
                Name = "Technology",
                Description = "Another description"
            };

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.CreateCategoryAsync(dto));

            Assert.Equal("Category already exists.", exception.Message);
            Assert.Equal(400, exception.StatusCode);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnCategoriesOrderedByName()
        {
            using var context = CreateDbContext();

            context.Categories.AddRange(
                new Category
                {
                    Name = "Technology",
                    Description = "Tech"
                },
                new Category
                {
                    Name = "Art",
                    Description = "Art"
                },
                new Category
                {
                    Name = "Business",
                    Description = "Business"
                }
            );

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var result = (await service.GetAllCategoriesAsync()).ToList();

            Assert.Equal(3, result.Count);

            Assert.Equal("Art", result[0].Name);
            Assert.Equal("Business", result[1].Name);
            Assert.Equal("Technology", result[2].Name);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnEmptyList_WhenNoCategoriesExist()
        {
            using var context = CreateDbContext();
            var service = new CategoryService(context);

            var result = (await service.GetAllCategoriesAsync()).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            using var context = CreateDbContext();

            var category = new Category
            {
                Name = "Technology",
                Description = "Tech blogs"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var result = await service.GetCategoryByIdAsync(category.Id);

            Assert.NotNull(result);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal("Technology", result.Name);
            Assert.Equal("Tech blogs", result.Description);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            using var context = CreateDbContext();
            var service = new CategoryService(context);

            var result = await service.GetCategoryByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldUpdateCategory_WhenDataIsValid()
        {
            using var context = CreateDbContext();

            var category = new Category
            {
                Name = "Technology",
                Description = "Old description"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var dto = new CreateCategoryDto
            {
                Name = "Programming",
                Description = "Programming blogs"
            };

            var result = await service.UpdateCategoryAsync(
                category.Id,
                dto);

            Assert.Equal(category.Id, result.Id);
            Assert.Equal("Programming", result.Name);
            Assert.Equal("Programming blogs", result.Description);

            var updatedCategory = await context.Categories
                .FirstAsync(c => c.Id == category.Id);

            Assert.Equal("Programming", updatedCategory.Name);
            Assert.Equal("Programming blogs", updatedCategory.Description);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            using var context = CreateDbContext();
            var service = new CategoryService(context);

            var dto = new CreateCategoryDto
            {
                Name = "Technology",
                Description = "Tech"
            };

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.UpdateCategoryAsync(999, dto));

            Assert.Equal("Category not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowException_WhenNewNameAlreadyExists()
        {
            using var context = CreateDbContext();

            var category1 = new Category
            {
                Name = "Technology",
                Description = "Tech"
            };

            var category2 = new Category
            {
                Name = "Business",
                Description = "Business"
            };

            context.Categories.AddRange(category1, category2);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var dto = new CreateCategoryDto
            {
                Name = "Business",
                Description = "Updated description"
            };

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.UpdateCategoryAsync(category1.Id, dto));

            Assert.Equal("Category already exists.", exception.Message);
            Assert.Equal(400, exception.StatusCode);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldAllowKeepingSameName()
        {
            using var context = CreateDbContext();

            var category = new Category
            {
                Name = "Technology",
                Description = "Old description"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var dto = new CreateCategoryDto
            {
                Name = "Technology",
                Description = "New description"
            };

            var result = await service.UpdateCategoryAsync(
                category.Id,
                dto);

            Assert.Equal(category.Id, result.Id);
            Assert.Equal("Technology", result.Name);
            Assert.Equal("New description", result.Description);
        }

        [Fact]
        public async Task GetBlogsByCategoryAsync_ShouldReturnBlogsForCategory()
        {
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "Ali",
                Email = "ali@test.com",
                PasswordHash = "hashed",
                Role = "User"
            };

            var category = new Category
            {
                Name = "Technology",
                Description = "Tech blogs"
            };

            context.Users.Add(user);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var blog1 = new Blog
            {
                Title = "First Blog",
                Content = "First content",
                UserId = user.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var blog2 = new Blog
            {
                Title = "Second Blog",
                Content = "Second content",
                UserId = user.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.AddRange(blog1, blog2);

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var result = (await service.GetBlogsByCategoryAsync(
                category.Id)).ToList();

            Assert.Equal(2, result.Count);

            Assert.Equal("Second Blog", result[0].Title);
            Assert.Equal("First Blog", result[1].Title);

            Assert.All(result, blog =>
            {
                Assert.Equal(category.Id, blog.CategoryId);
                Assert.Equal("Technology", blog.CategoryName);
                Assert.Equal("Ali", blog.AuthorName);
            });
        }

        [Fact]
        public async Task GetBlogsByCategoryAsync_ShouldReturnEmptyList_WhenCategoryHasNoBlogs()
        {
            using var context = CreateDbContext();

            var category = new Category
            {
                Name = "Technology",
                Description = "Tech"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var result = (await service.GetBlogsByCategoryAsync(
                category.Id)).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBlogsByCategoryAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            using var context = CreateDbContext();
            var service = new CategoryService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.GetBlogsByCategoryAsync(999));

            Assert.Equal("Category not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldDeleteCategory_WhenCategoryHasNoBlogs()
        {
            using var context = CreateDbContext();

            var category = new Category
            {
                Name = "Technology",
                Description = "Tech"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var categoryId = category.Id;

            var service = new CategoryService(context);

            await service.DeleteCategoryAsync(categoryId);

            var deletedCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            Assert.Null(deletedCategory);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            using var context = CreateDbContext();
            var service = new CategoryService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.DeleteCategoryAsync(999));

            Assert.Equal("Category not found.", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowException_WhenCategoryContainsBlogs()
        {
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "Ali",
                Email = "ali@test.com",
                PasswordHash = "hashed",
                Role = "User"
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
                Content = "Test content",
                UserId = user.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => service.DeleteCategoryAsync(category.Id));

            Assert.Equal(
                "Cannot delete a category that contains blogs.",
                exception.Message);

            Assert.Equal(400, exception.StatusCode);

            var existingCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == category.Id);

            Assert.NotNull(existingCategory);
        }
    }
}