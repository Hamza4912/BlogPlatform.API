using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.API.DTOs.Blog
{
    public class AddCommentDto
    {
        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = string.Empty;
    }
}