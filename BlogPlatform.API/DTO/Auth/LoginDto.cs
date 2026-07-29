using System.ComponentModel.DataAnnotations;    
namespace BlogPlatform.API.DTO.Auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
