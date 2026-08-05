using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.API.DTOs.Blog
{
    public class CreateBlogDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }
    }
}