namespace BlogPlatform.API.DTOs.Common
{
    public class PaginatedResponseDto<T>
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public IEnumerable<T> Items { get; set; } = new List<T>();
    }
}