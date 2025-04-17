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

            // Call the external seeder for localization strings
            LocalizationDataSeeder.Seed(builder);

            // Other potential seeding or model configurations can go here
        }
    }
}
