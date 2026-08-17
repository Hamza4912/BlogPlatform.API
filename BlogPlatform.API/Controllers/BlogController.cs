using BlogPlatform.API.DTOs.Blog;
using BlogPlatform.API.DTOs.Common;
using BlogPlatform.API.Services;
using BlogPlatform.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly IBlogLikeService _blogLikeService;

        public BlogController(IBlogService blogService,IBlogLikeService blogLikeService)
        {
            _blogService = blogService;
            _blogLikeService = blogLikeService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateBlog(CreateBlogDto createBlogDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var result = await _blogService.CreateBlogAsync(createBlogDto, userId);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogsAsync([FromQuery] PaginationDto paginationDto)
        {
            var blogs = await _blogService.GetBlogsAsync(paginationDto);

            return Ok(blogs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            var blog = await _blogService.GetBlogByIdAsync(id);

            if (blog == null)
            {
                return NotFound(new
                {
                    message = "Blog not found."
                });
            }

            return Ok(blog);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlog(
    int id,
    CreateBlogDto updateBlogDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var result = await _blogService.UpdateBlogAsync(
                id,
                updateBlogDto,
                userId);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            await _blogService.DeleteBlogAsync(id, userId);

            return Ok(new
            {
                message = "Blog deleted successfully."
            });
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeBlog(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            await _blogLikeService.LikeBlogAsync(id, userId);

            return Ok(new
            {
                message = "Blog liked successfully."
            });
        }

        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikeBlog(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            await _blogLikeService.UnlikeBlogAsync(id, userId);

            return Ok(new
            {
                message = "Blog unliked successfully."
            });
        }
    }
}