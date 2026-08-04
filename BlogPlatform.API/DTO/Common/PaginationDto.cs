using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.API.DTOs.Common
{
    public class PaginationDto
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public string? SortBy { get; set; } = "newest";

        public string? AuthorName { get; set; }
    }
}