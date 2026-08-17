using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace BlogPlatform.API.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<BlogLike> Likes { get; set; } = new List<BlogLike>();
    }
}
