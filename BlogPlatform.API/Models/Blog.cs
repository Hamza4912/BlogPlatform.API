using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.API.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } 

        //FK
        public int UserId { get; set; }

        //Navigation Properties for user(Each blog is associated with one user)
        public User User { get; set; } = null!;

        // Foreign Key
        public int CategoryId { get; set; }

        // Navigation Property
        public Category Category { get; set; } = null!;

        //Navigation property for comments
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
