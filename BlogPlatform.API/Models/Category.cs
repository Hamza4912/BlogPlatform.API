using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.API.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation Property
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    }
}