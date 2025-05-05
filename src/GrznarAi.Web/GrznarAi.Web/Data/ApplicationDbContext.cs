using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using GrznarAi.Web.Api.Models;

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
        public DbSet<AiNewsItem> AiNewsItems { get; set; }
        public DbSet<AiNewsSource> AiNewsSources { get; set; }
        public DbSet<AiNewsError> AiNewsErrors { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<GlobalSetting> GlobalSettings { get; set; }
        public DbSet<ApplicationPermission> ApplicationPermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteCategory> NoteCategories { get; set; }
        public DbSet<NoteCategoryRelation> NoteCategoryRelations { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<EmailTemplateTranslation> EmailTemplateTranslations { get; set; }
        public DbSet<WeatherHistory> WeatherHistory { get; set; }

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
            
            // Konfigurace pro AiNewsItem - vytvoří index pro rychlejší vyhledávání
            builder.Entity<AiNewsItem>()
                .HasIndex(n => n.PublishedDate);
            
            builder.Entity<AiNewsItem>()
                .HasIndex(n => n.ImportedDate);
                
            // Přidaný index pro rychlejší vyhledávání podle titulku
            builder.Entity<AiNewsItem>()
                .HasIndex(n => new { n.TitleEn, n.ImportedDate });

            // Konfigurace pro AiNewsSource
            builder.Entity<AiNewsSource>()
                .HasIndex(s => s.Name)
                .IsUnique();

            builder.Entity<AiNewsSource>()
                .HasIndex(s => s.Url);

            builder.Entity<AiNewsSource>()
                .HasIndex(s => s.Type);

            // Konfigurace pro AiNewsError
            builder.Entity<AiNewsError>()
                .HasIndex(e => e.OccurredAt);

            builder.Entity<AiNewsError>()
                .HasOne(e => e.Source)
                .WithMany()
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Konfigurace pro Note
            builder.Entity<Note>()
                .HasOne(n => n.ApplicationUser)
                .WithMany()
                .HasForeignKey(n => n.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfigurace pro NoteCategory
            builder.Entity<NoteCategory>()
                .HasOne(nc => nc.ApplicationUser)
                .WithMany()
                .HasForeignKey(nc => nc.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfigurace pro vztah Note-Category
            builder.Entity<Note>()
                .HasMany(n => n.Categories)
                .WithMany(c => c.Notes)
                .UsingEntity<NoteCategoryRelation>(
                    j => j
                        .HasOne(ncr => ncr.NoteCategory)
                        .WithMany()
                        .HasForeignKey(ncr => ncr.NoteCategoryId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne(ncr => ncr.Note)
                        .WithMany()
                        .HasForeignKey(ncr => ncr.NoteId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(t => t.Id);
                        j.HasIndex(t => new { t.NoteId, t.NoteCategoryId }).IsUnique();
                    });

            // Konfigurace pro UserPermission
            builder.Entity<UserPermission>()
                .HasOne(up => up.ApplicationUser)
                .WithMany()
                .HasForeignKey(up => up.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserPermission>()
                .HasOne(up => up.ApplicationPermission)
                .WithMany()
                .HasForeignKey(up => up.ApplicationPermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfigurace pro EmailTemplate
            builder.Entity<EmailTemplate>()
                .HasIndex(et => et.TemplateKey)
                .IsUnique();

            builder.Entity<EmailTemplate>()
                .HasMany(e => e.Translations)
                .WithOne(t => t.EmailTemplate)
                .HasForeignKey(t => t.EmailTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
