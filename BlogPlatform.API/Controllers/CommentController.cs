using BlogPlatform.API.DTOs.Comment;
using BlogPlatform.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost("/api/blog/{blogId}/comments")]
        public async Task<IActionResult> CreateComment(
        int blogId,
        CreateCommentDto createCommentDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var result = await _commentService.CreateCommentAsync(
                blogId,
                createCommentDto,
                userId);

            return Ok(result);
        }

        [HttpGet("/api/blog/{blogId}/comments")]
        public async Task<IActionResult> GetCommentsByBlog(int blogId)
        {
            var comments = await _commentService.GetCommentsByBlogAsync(blogId);

            return Ok(comments);
        }

        [HttpPut("/api/comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(
    int commentId,
    CreateCommentDto updateCommentDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var result = await _commentService.UpdateCommentAsync(
                commentId,
                updateCommentDto,
                userId);

            return Ok(result);
        }

        [HttpDelete("/api/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            await _commentService.DeleteCommentAsync(commentId, userId);

            return Ok(new
            {
                message = "Comment deleted successfully."
            });
        }
    }
}