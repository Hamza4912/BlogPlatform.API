using BlogPlatform.API.DTOs.Blog;

namespace BlogPlatform.API.Services.Interfaces
{
    public interface IBlogService
    {
        Task<BlogResponseDto> CreateBlogAsync(CreateBlogDto createBlogDto, int userId);

        Task<IEnumerable<BlogResponseDto>> GetAllBlogsAsync();

        Task<BlogResponseDto?> GetBlogByIdAsync(int id);

        Task<BlogResponseDto> UpdateBlogAsync(int id, CreateBlogDto updateBlogDto, int userId);

        Task DeleteBlogAsync(int id, int userId);
    }
}