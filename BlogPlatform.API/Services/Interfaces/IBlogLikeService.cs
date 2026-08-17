using BlogPlatform.API.DTOs.BlogLike;

namespace BlogPlatform.API.Services.Interfaces
{
    public interface IBlogLikeService
    {
        Task LikeBlogAsync(int blogId, int userId);

        Task UnlikeBlogAsync(int blogId, int userId);

        Task<LikeResponseDto> GetLikesAsync(int blogId);
    }
}