using BlogPlatform.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogPlatform.API.DTOs.Blog;
using System.Security.Claims;   

namespace BlogPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
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
        public async Task<IActionResult> GetAllBlogs()
        {
            var blogs = await _blogService.GetAllBlogsAsync();

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
    }
}