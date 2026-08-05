using BlogPlatform.API.DTOs.Category;

namespace BlogPlatform.API.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);

        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();

        Task<CategoryResponseDto?> GetCategoryByIdAsync(int id);

        Task<CategoryResponseDto> UpdateCategoryAsync(int id, CreateCategoryDto updateCategoryDto);

        Task<IEnumerable<BlogResponseDto>> GetBlogsByCategoryAsync(int categoryId);

        Task DeleteCategoryAsync(int id);
    }
}