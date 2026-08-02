using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.API.DTOs.Comment
{
    public class CreateCommentDto
    {
        [Required]
        public string Text { get; set; } = string.Empty;
    }
}