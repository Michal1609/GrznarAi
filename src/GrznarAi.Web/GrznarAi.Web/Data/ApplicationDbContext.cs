using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GrznarAi.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectContent> ProjectContents { get; set; }
        public DbSet<LocalizationString> LocalizationStrings { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogContent> BlogContents { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentVote> CommentVotes { get; set; }
        public DbSet<BlogVote> BlogVotes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Konfigurace pro Comment - každý komentář patří buď k blogu nebo je podkomentářem
            builder.Entity<Comment>()
                .HasOne(c => c.Blog)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction); // Podkomentáře nejsou automaticky smazány při smazání nadřazeného komentáře

            // Konfigurace pro CommentVote - zajištění pouze jednoho hlasu na komentář od uživatele nebo cookie
            builder.Entity<CommentVote>()
                .HasOne(cv => cv.Comment)
                .WithMany(c => c.Votes)
                .HasForeignKey(cv => cv.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfigurace pro BlogVote - zajištění pouze jednoho hlasu na blog od uživatele nebo cookie
            builder.Entity<BlogVote>()
                .HasOne(bv => bv.Blog)
                .WithMany(b => b.Votes)
                .HasForeignKey(bv => bv.BlogId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
