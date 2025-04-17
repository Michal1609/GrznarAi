using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GrznarAi.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<LocalizationString> LocalizationStrings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Localization Strings for Home Page
            builder.Entity<LocalizationString>().HasData(
                // Page Title
                new LocalizationString { Id = 1, Key = "HomePage.Title", ValueCs = "GrznarAI - Osobní Web", ValueEn = "GrznarAI - Personal Website", Description = "Home page title" },
                // Carousel 1
                new LocalizationString { Id = 2, Key = "HomePage.Carousel1.Title", ValueCs = "Vítejte na GrznarAI", ValueEn = "Welcome to GrznarAI", Description = "Home Carousel 1 Title" },
                new LocalizationString { Id = 3, Key = "HomePage.Carousel1.Lead", ValueCs = "Osobní web s blogem, projekty a meteo daty", ValueEn = "Personal website with blog, projects and meteo data", Description = "Home Carousel 1 Lead Text" },
                new LocalizationString { Id = 4, Key = "HomePage.Carousel.ReadBlogButton", ValueCs = "Číst Blog", ValueEn = "Read Blog", Description = "Carousel Button - Read Blog" },
                new LocalizationString { Id = 5, Key = "HomePage.Carousel.ViewProjectsButton", ValueCs = "Zobrazit Projekty", ValueEn = "View Projects", Description = "Carousel Button - View Projects" },
                // Carousel 2
                new LocalizationString { Id = 6, Key = "HomePage.Carousel2.Title", ValueCs = "Prozkoumejte Mé Projekty", ValueEn = "Explore My Projects", Description = "Home Carousel 2 Title" },
                new LocalizationString { Id = 7, Key = "HomePage.Carousel2.Lead", ValueCs = "Podívejte se na mé nejnovější GitHub projekty a experimenty", ValueEn = "Check out my latest GitHub projects and experiments", Description = "Home Carousel 2 Lead Text" },
                // Carousel 3
                new LocalizationString { Id = 8, Key = "HomePage.Carousel3.Title", ValueCs = "Meteo Data", ValueEn = "Meteo Data", Description = "Home Carousel 3 Title" },
                new LocalizationString { Id = 9, Key = "HomePage.Carousel3.Lead", ValueCs = "Prozkoumejte statistiky počasí z mých osobních meteostanic", ValueEn = "Explore weather statistics from my personal meteo stations", Description = "Home Carousel 3 Lead Text" },
                new LocalizationString { Id = 10, Key = "HomePage.Carousel.ViewMeteoButton", ValueCs = "Zobrazit Meteo Data", ValueEn = "View Meteo Data", Description = "Carousel Button - View Meteo Data" },
                // Carousel Controls
                new LocalizationString { Id = 11, Key = "HomePage.Carousel.Previous", ValueCs = "Předchozí", ValueEn = "Previous", Description = "Carousel Control - Previous" },
                new LocalizationString { Id = 12, Key = "HomePage.Carousel.Next", ValueCs = "Další", ValueEn = "Next", Description = "Carousel Control - Next" },
                // Featured Content - Blog
                new LocalizationString { Id = 13, Key = "HomePage.Featured.Blog.Title", ValueCs = "Blog", ValueEn = "Blog", Description = "Featured Card Title - Blog" },
                new LocalizationString { Id = 14, Key = "HomePage.Featured.Blog.Text", ValueCs = "Sdílejte články ve formátu Markdown s full-text vyhledáváním a komentáři.", ValueEn = "Share articles in Markdown format with full-text search and commenting capabilities.", Description = "Featured Card Text - Blog" },
                // Featured Content - Projects
                new LocalizationString { Id = 15, Key = "HomePage.Featured.Projects.Title", ValueCs = "Projekty", ValueEn = "Projects", Description = "Featured Card Title - Projects" },
                new LocalizationString { Id = 16, Key = "HomePage.Featured.Projects.Text", ValueCs = "Prozkoumejte mé GitHub projekty s dokumentací, changelogy a demy.", ValueEn = "Explore my GitHub projects with documentation, changelogs, and demos.", Description = "Featured Card Text - Projects" },
                // Featured Content - Meteo
                new LocalizationString { Id = 17, Key = "HomePage.Featured.Meteo.Title", ValueCs = "Meteo Data", ValueEn = "Meteo Data", Description = "Featured Card Title - Meteo" },
                new LocalizationString { Id = 18, Key = "HomePage.Featured.Meteo.Text", ValueCs = "Získejte přístup ke statistikám počasí a datům z osobních meteostanic.", ValueEn = "Access weather statistics and data from personal meteo stations.", Description = "Featured Card Text - Meteo" },
                // Latest Blog Posts Section
                new LocalizationString { Id = 19, Key = "HomePage.LatestPosts.Title", ValueCs = "Nejnovější Příspěvky na Blogu", ValueEn = "Latest Blog Posts", Description = "Latest Posts Section Title" },
                new LocalizationString { Id = 20, Key = "HomePage.LatestPosts.ReadMore", ValueCs = "Číst Více", ValueEn = "Read More", Description = "Latest Posts Button - Read More" },
                new LocalizationString { Id = 21, Key = "HomePage.LatestPosts.ViewAll", ValueCs = "Zobrazit Všechny Příspěvky", ValueEn = "View All Posts", Description = "Latest Posts Button - View All" }
            );
        }
    }
}
