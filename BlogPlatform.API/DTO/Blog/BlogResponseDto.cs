namespace BlogPlatform.API.DTOs.Blog
{
    public class BlogResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string AuthorName { get; set; } = string.Empty;
    }
}