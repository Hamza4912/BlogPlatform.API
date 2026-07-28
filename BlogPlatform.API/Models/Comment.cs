using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.API.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key
    public int UserId { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;

    // Foreign Key
    public int BlogId { get; set; }

    // Navigation Property
    public Blog Blog { get; set; } = null!;
}