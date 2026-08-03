using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.API.DTOs.User
{
    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}