namespace BlogPlatform.API.DTO.Admin
{
    public class AdminDashboardResponseDto
    {
        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int DeactivatedUsers { get; set; }

        public int TotalBlogs { get; set; }

        public int TotalComments { get; set; }

        public int TotalCategories { get; set; }

        public int TotalLikes { get; set; }
    }
}