namespace BlogPlatform.API.DTOs.Comment
{
    public class CommentResponseDto
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public int BlogId { get; set; }
    }
}