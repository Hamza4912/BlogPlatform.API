using BlogPlatform.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
     : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<BlogLike> BlogLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BlogLike>()
                .HasOne(bl => bl.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(bl => bl.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BlogLike>()
                .HasOne(bl => bl.Blog)
                .WithMany(b => b.Likes)
                .HasForeignKey(bl => bl.BlogId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BlogLike>()
            .HasIndex(bl => new { bl.UserId, bl.BlogId })
            .IsUnique();

            base.OnModelCreating(modelBuilder);
        }

        
    }
}
