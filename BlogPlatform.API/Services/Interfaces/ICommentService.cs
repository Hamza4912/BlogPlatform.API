using BlogPlatform.API.DTOs.Comment;

namespace BlogPlatform.API.Services.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponseDto> CreateCommentAsync(
            int blogId,
            CreateCommentDto createCommentDto,
            int userId);

        Task<IEnumerable<CommentResponseDto>> GetCommentsByBlogAsync(int blogId);

        Task<CommentResponseDto> UpdateCommentAsync(
            int commentId,
            CreateCommentDto updateCommentDto,
            int userId);

        Task DeleteCommentAsync(int commentId, int userId);
    }
}