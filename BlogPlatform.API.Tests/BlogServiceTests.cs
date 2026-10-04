using BlogPlatform.API.Data;
using BlogPlatform.API.DTOs.Blog;
using BlogPlatform.API.DTOs.Common;
using BlogPlatform.API.Models;
using BlogPlatform.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Tests
{
    public class BlogServiceTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
        
        [Fact]
        public async Task CreateBlogAsync_ShouldCreateBlog_WhenDataIsValid()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "BlogAuthor",
                Email = "author@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Technology"
            };

            context.Users.Add(user);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var createBlogDto = new CreateBlogDto
            {
                Title = "My First Blog",
                Content = "This is my first blog content.",
                CategoryId = category.Id
            };

            // Act
            var result = await blogService.CreateBlogAsync(
                createBlogDto,
                user.Id);

            // Assert
            Assert.NotNull(result);

            Assert.Equal("My First Blog", result.Title);
            Assert.Equal(
                "This is my first blog content.",
                result.Content);

            Assert.Equal(user.Id, result.UserId);
            Assert.Equal("BlogAuthor", result.AuthorName);

            Assert.Equal(category.Id, result.CategoryId);
            Assert.Equal("Technology", result.CategoryName);

            var blog = await context.Blogs
                .FirstOrDefaultAsync(b => b.Id == result.Id);

            Assert.NotNull(blog);
            Assert.Equal("My First Blog", blog.Title);
            Assert.Equal(user.Id, blog.UserId);
            Assert.Equal(category.Id, blog.CategoryId);
        }
        [Fact]
        public async Task CreateBlogAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "BlogAuthor",
                Email = "author@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var createBlogDto = new CreateBlogDto
            {
                Title = "Test Blog",
                Content = "Test content.",
                CategoryId = 999
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await blogService.CreateBlogAsync(
                    createBlogDto,
                    user.Id));

            // Verify blog was not created
            Assert.Equal(0, await context.Blogs.CountAsync());
        }
        [Fact]
        public async Task GetBlogByIdAsync_ShouldReturnBlog_WhenBlogExists()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "GetAuthor",
                Email = "getauthor@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Programming"
            };

            context.Users.Add(user);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "C# Testing",
                Content = "Learning xUnit testing.",
                UserId = user.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            // Act
            var result = await blogService.GetBlogByIdAsync(blog.Id);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(blog.Id, result.Id);
            Assert.Equal("C# Testing", result.Title);
            Assert.Equal("Learning xUnit testing.", result.Content);

            Assert.Equal(user.Id, result.UserId);
            Assert.Equal("GetAuthor", result.AuthorName);

            Assert.Equal(category.Id, result.CategoryId);
            Assert.Equal("Programming", result.CategoryName);
        }
        [Fact]
        public async Task GetBlogByIdAsync_ShouldReturnNull_WhenBlogDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();

            var blogService = new BlogService(context);

            // Act
            var result = await blogService.GetBlogByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateBlogAsync_ShouldUpdateBlog_WhenUserIsOwner()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "BlogOwner",
                Email = "owner@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var oldCategory = new Category
            {
                Name = "Old Category"
            };

            var newCategory = new Category
            {
                Name = "New Category"
            };

            context.Users.Add(user);
            context.Categories.AddRange(oldCategory, newCategory);

            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Old Title",
                Content = "Old Content",
                UserId = user.Id,
                CategoryId = oldCategory.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var updateDto = new CreateBlogDto
            {
                Title = "Updated Title",
                Content = "Updated Content",
                CategoryId = newCategory.Id
            };

            // Act
            var result = await blogService.UpdateBlogAsync(
                blog.Id,
                updateDto,
                user.Id);

            // Assert
            Assert.NotNull(result);

            Assert.Equal("Updated Title", result.Title);
            Assert.Equal("Updated Content", result.Content);

            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(newCategory.Id, result.CategoryId);
            Assert.Equal("New Category", result.CategoryName);

            var updatedBlog = await context.Blogs
                .FirstAsync(b => b.Id == blog.Id);

            Assert.Equal("Updated Title", updatedBlog.Title);
            Assert.Equal("Updated Content", updatedBlog.Content);
            Assert.Equal(newCategory.Id, updatedBlog.CategoryId);
        }
        [Fact]
        public async Task UpdateBlogAsync_ShouldThrowException_WhenBlogDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();

            var category = new Category
            {
                Name = "Technology"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var updateDto = new CreateBlogDto
            {
                Title = "Updated Title",
                Content = "Updated Content",
                CategoryId = category.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await blogService.UpdateBlogAsync(
                    999,
                    updateDto,
                    1));
        }
        [Fact]
        public async Task UpdateBlogAsync_ShouldThrowException_WhenUserIsNotOwner()
        {
            // Arrange
            using var context = CreateDbContext();

            var owner = new User
            {
                Name = "Owner",
                Email = "owner@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var otherUser = new User
            {
                Name = "OtherUser",
                Email = "other@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Technology"
            };

            context.Users.AddRange(owner, otherUser);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Owner Blog",
                Content = "Original content",
                UserId = owner.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var updateDto = new CreateBlogDto
            {
                Title = "Hacked Title",
                Content = "Changed by another user",
                CategoryId = category.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await blogService.UpdateBlogAsync(
                    blog.Id,
                    updateDto,
                    otherUser.Id));
        }
        [Fact]
        public async Task UpdateBlogAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "UpdateUser",
                Email = "update@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Technology"
            };

            context.Users.Add(user);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Original Blog",
                Content = "Original content",
                UserId = user.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var updateDto = new CreateBlogDto
            {
                Title = "Updated Blog",
                Content = "Updated content",
                CategoryId = 999
            };

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await blogService.UpdateBlogAsync(
                    blog.Id,
                    updateDto,
                    user.Id));
        }
        [Fact]
        public async Task DeleteBlogAsync_ShouldDeleteBlog_WhenUserIsOwner()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "DeleteOwner",
                Email = "deleteowner@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Technology"
            };

            context.Users.Add(user);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Blog To Delete",
                Content = "Delete this blog.",
                UserId = user.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            // Act
            await blogService.DeleteBlogAsync(
                blog.Id,
                user.Id);

            // Assert
            var deletedBlog = await context.Blogs
                .FirstOrDefaultAsync(b => b.Id == blog.Id);

            Assert.Null(deletedBlog);
        }
        [Fact]
        public async Task DeleteBlogAsync_ShouldThrowException_WhenBlogDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();

            var blogService = new BlogService(context);

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await blogService.DeleteBlogAsync(
                    999,
                    1));
        }
        [Fact]
        public async Task DeleteBlogAsync_ShouldThrowException_WhenUserIsNotOwner()
        {
            // Arrange
            using var context = CreateDbContext();

            var owner = new User
            {
                Name = "Owner",
                Email = "deleteowner@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var otherUser = new User
            {
                Name = "OtherUser",
                Email = "otherdelete@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Technology"
            };

            context.Users.AddRange(owner, otherUser);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var blog = new Blog
            {
                Title = "Protected Blog",
                Content = "Owner content",
                UserId = owner.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            // Act & Assert
            await Assert.ThrowsAsync<BlogPlatform.API.Exceptions.ApiException>(
                async () => await blogService.DeleteBlogAsync(
                    blog.Id,
                    otherUser.Id));

            // Verify blog still exists
            Assert.NotNull(
                await context.Blogs.FirstOrDefaultAsync(
                    b => b.Id == blog.Id));
        }
        [Fact]
        public async Task GetBlogsAsync_ShouldReturnPaginatedBlogs()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "PaginationAuthor",
                Email = "pagination@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Technology"
            };

            context.Users.Add(user);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            context.Blogs.AddRange(
                new Blog
                {
                    Title = "Blog 1",
                    Content = "Content 1",
                    UserId = user.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-3)
                },
                new Blog
                {
                    Title = "Blog 2",
                    Content = "Content 2",
                    UserId = user.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-2)
                },
                new Blog
                {
                    Title = "Blog 3",
                    Content = "Content 3",
                    UserId = user.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1)
                });

            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 2
            };

            // Act
            var result = await blogService.GetBlogsAsync(paginationDto);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.TotalPages);

            Assert.Equal(2, result.Items.Count());
        }
        [Fact]
        public async Task GetBlogsAsync_ShouldFilterBlogs_WhenSearchIsProvided()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "SearchAuthor",
                Email = "search@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Technology"
            };

            context.Users.Add(user);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            context.Blogs.AddRange(
                new Blog
                {
                    Title = "CSharp Programming",
                    Content = "Learn C#.",
                    UserId = user.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Blog
                {
                    Title = "Cooking Guide",
                    Content = "Learn cooking.",
                    UserId = user.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1)
                });

            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                Search = "CSharp"
            };

            // Act
            var result = await blogService.GetBlogsAsync(paginationDto);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);

            Assert.Equal(
                "CSharp Programming",
                result.Items.First().Title);
        }
        [Fact]
        public async Task GetBlogsAsync_ShouldFilterBlogs_WhenAuthorNameIsProvided()
        {
            // Arrange
            using var context = CreateDbContext();

            var author1 = new User
            {
                Name = "Ali",
                Email = "ali@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var author2 = new User
            {
                Name = "Ahmed",
                Email = "ahmed@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Technology"
            };

            context.Users.AddRange(author1, author2);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            context.Blogs.AddRange(
                new Blog
                {
                    Title = "Ali Blog",
                    Content = "Ali content",
                    UserId = author1.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Blog
                {
                    Title = "Ahmed Blog",
                    Content = "Ahmed content",
                    UserId = author2.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1)
                });

            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                AuthorName = "Ali"
            };

            // Act
            var result = await blogService.GetBlogsAsync(paginationDto);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);

            Assert.Equal(
                "Ali Blog",
                result.Items.First().Title);

            Assert.Equal(
                "Ali",
                result.Items.First().AuthorName);
        }

        [Fact]
        public async Task GetBlogsAsync_ShouldReturnOldestFirst_WhenSortByIsOldest()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Name = "SortAuthor",
                Email = "sort@example.com",
                PasswordHash = "hashed-password",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var category = new Category
            {
                Name = "Technology"
            };

            context.Users.Add(user);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            context.Blogs.AddRange(
                new Blog
                {
                    Title = "Old Blog",
                    Content = "Old",
                    UserId = user.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Blog
                {
                    Title = "New Blog",
                    Content = "New",
                    UserId = user.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var blogService = new BlogService(context);

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "oldest"
            };

            // Act
            var result = await blogService.GetBlogsAsync(paginationDto);

            // Assert
            Assert.Equal(2, result.TotalCount);

            var items = result.Items.ToList();

            Assert.Equal("Old Blog", items[0].Title);
            Assert.Equal("New Blog", items[1].Title);
        }


    }
}