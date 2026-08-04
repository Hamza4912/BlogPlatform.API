using BlogPlatform.API.DTOs.Blog;
using BlogPlatform.API.DTOs.Common;

namespace BlogPlatform.API.Services.Interfaces
{
    public interface IBlogService
    {
        Task<BlogResponseDto> CreateBlogAsync(CreateBlogDto createBlogDto, int userId);
        Task<PaginatedResponseDto<BlogResponseDto>> GetBlogsAsync(PaginationDto paginationDto);
        Task<BlogResponseDto?> GetBlogByIdAsync(int id);

        Task<BlogResponseDto> UpdateBlogAsync(int id, CreateBlogDto updateBlogDto, int userId);

        Task DeleteBlogAsync(int id, int userId);

        
    }
}