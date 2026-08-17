namespace BlogPlatform.API.Models
{
    public class BlogLike
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        // Foreign Key
        public int BlogId { get; set; }

        public Blog Blog { get; set; } = null!;
    }
}