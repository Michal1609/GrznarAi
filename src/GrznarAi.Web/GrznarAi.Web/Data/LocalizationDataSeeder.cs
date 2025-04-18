using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Data
{
    public static class LocalizationDataSeeder
    {
        public static async Task SeedAsync(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            using var context = await contextFactory.CreateDbContextAsync();
            
            var localizationStrings = new List<LocalizationString>();
            var existingKeys = new HashSet<string>(); // Key_LanguageCode

            // Helper function to add entries if not already present
            void AddEntry(string key, string valueCs, string valueEn, string? description)
            {
                if (existingKeys.Add($"{key}_cs"))
                {
                    localizationStrings.Add(new LocalizationString { Key = key, LanguageCode = "cs", Value = valueCs, Description = description });
                }
                if (existingKeys.Add($"{key}_en"))
                {
                    localizationStrings.Add(new LocalizationString { Key = key, LanguageCode = "en", Value = valueEn, Description = description });
                }
            }

            // --- Home Page Seed Data --- 
            AddEntry("HomePage.Title", "GrznarAI - Osobní Web", "GrznarAI - Personal Website", "Home page title");
            AddEntry("HomePage.Carousel1.Title", "Vítejte na GrznarAI", "Welcome to GrznarAI", "Home Carousel 1 Title");
            AddEntry("HomePage.Carousel1.Lead", "Osobní web s blogem, projekty a meteo daty", "Personal website with blog, projects and meteo data", "Home Carousel 1 Lead Text");
            AddEntry("HomePage.Carousel.ReadBlogButton", "Číst Blog", "Read Blog", "Carousel Button - Read Blog");
            AddEntry("HomePage.Carousel.ViewProjectsButton", "Zobrazit Projekty", "View Projects", "Carousel Button - View Projects");
            AddEntry("HomePage.Carousel2.Title", "Prozkoumejte Mé Projekty", "Explore My Projects", "Home Carousel 2 Title");
            AddEntry("HomePage.Carousel2.Lead", "Podívejte se na mé nejnovější GitHub projekty a experimenty", "Check out my latest GitHub projects and experiments", "Home Carousel 2 Lead Text");
            AddEntry("HomePage.Carousel3.Title", "Meteo Data", "Meteo Data", "Home Carousel 3 Title");
            AddEntry("HomePage.Carousel3.Lead", "Prozkoumejte statistiky počasí z mých osobních meteostanic", "Explore weather statistics from my personal meteo stations", "Home Carousel 3 Lead Text");
            AddEntry("HomePage.Carousel.ViewMeteoButton", "Zobrazit Meteo Data", "View Meteo Data", "Carousel Button - View Meteo Data");
            AddEntry("HomePage.Carousel.Previous", "Předchozí", "Previous", "Carousel Control - Previous");
            AddEntry("HomePage.Carousel.Next", "Další", "Next", "Carousel Control - Next");
            AddEntry("HomePage.Featured.Blog.Title", "Blog", "Blog", "Featured Card Title - Blog");
            AddEntry("HomePage.Featured.Blog.Text", "Sdílejte články ve formátu Markdown s full-text vyhledáváním a komentáři.", "Share articles in Markdown format with full-text search and commenting capabilities.", "Featured Card Text - Blog");
            AddEntry("HomePage.Featured.Projects.Title", "Projekty", "Projects", "Featured Card Title - Projects");
            AddEntry("HomePage.Featured.Projects.Text", "Prozkoumejte mé GitHub projekty s dokumentací, changelogy a demy.", "Explore my GitHub projects with documentation, changelogs, and demos.", "Featured Card Text - Projects");
            AddEntry("HomePage.Featured.Meteo.Title", "Meteo Data", "Meteo Data", "Featured Card Title - Meteo");
            AddEntry("HomePage.Featured.Meteo.Text", "Získejte přístup ke statistikám počasí a datům z osobních meteostanic.", "Access weather statistics and data from personal meteo stations.", "Featured Card Text - Meteo");
            AddEntry("HomePage.LatestPosts.Title", "Nejnovější Příspěvky na Blogu", "Latest Blog Posts", "Latest Posts Section Title");
            AddEntry("HomePage.LatestPosts.ReadMore", "Číst Více", "Read More", "Latest Posts Button - Read More");
            AddEntry("HomePage.LatestPosts.ViewAll", "Zobrazit Všechny Příspěvky", "View All Posts", "Latest Posts Button - View All");
            
            // --- NavMenu Seed Data ---
            AddEntry("NavMenu.Home", "Domů", "Home", "NavMenu Link - Home");
            AddEntry("NavMenu.Blog", "Blog", "Blog", "NavMenu Link - Blog");
            AddEntry("NavMenu.Projects", "Projekty", "Projects", "NavMenu Link - Projects");
            AddEntry("NavMenu.Meteo", "Meteo", "Meteo", "NavMenu Link - Meteo");
            AddEntry("NavMenu.Admin.Title", "Administrace", "Administration", "NavMenu Dropdown - Admin");
            AddEntry("NavMenu.Admin.Projects", "Projekty", "Projects", "NavMenu Admin Link - Projects");
            AddEntry("NavMenu.Admin.Localization", "Lokalizace", "Localization", "NavMenu Admin Link - Localization");
            AddEntry("NavMenu.User.ManageAccount", "Správa účtu", "Manage Account", "NavMenu User Dropdown - Manage");
            AddEntry("NavMenu.User.Logout", "Odhlásit se", "Logout", "NavMenu User Dropdown - Logout");
            AddEntry("NavMenu.Auth.Register", "Registrovat", "Register", "NavMenu Auth Link - Register");
            AddEntry("NavMenu.Auth.Login", "Přihlásit se", "Login", "NavMenu Auth Link - Login");

            // --- Footer Seed Data ---
            AddEntry("Footer.Links", "Odkazy", "Links", "Footer Heading - Links");
            AddEntry("Footer.Connect", "Spojte se", "Connect", "Footer Heading - Connect");
            AddEntry("Footer.GitHub", "GitHub", "GitHub", "Footer Link - GitHub"); // Same text, but good practice to have key
            AddEntry("Footer.Contact", "Kontakt", "Contact", "Footer Link - Contact");
            AddEntry("Footer.Copyright", "&copy; {0} GrznarAI. Všechna práva vyhrazena.", "&copy; {0} GrznarAI. All rights reserved.", "Footer Copyright Text (with year placeholder {0})");
            
            // --- Blog Seed Data ---
            AddEntry("Blog.Title", "Blog", "Blog", "Blog page title");
            AddEntry("Blog.Loading", "Načítání...", "Loading...", "Blog loading message");
            AddEntry("Blog.NoPostsFound", "Nebyly nalezeny žádné příspěvky.", "No posts found.", "Message when no blog posts are found");
            AddEntry("Blog.SearchResults", "Výsledky vyhledávání pro: {0}", "Search results for: {0}", "Search results message with search term placeholder");
            AddEntry("Blog.ClearSearch", "Vymazat vyhledávání", "Clear search", "Clear search button text");
            AddEntry("Blog.Search.Placeholder", "Hledat v blogu...", "Search blog...", "Search input placeholder");
            AddEntry("Blog.Search.Button", "Hledat", "Search", "Search button text");
            AddEntry("Blog.PopularTags", "Populární štítky", "Popular Tags", "Popular tags section title");
            AddEntry("Blog.Archive", "Archiv", "Archive", "Archive section title");
            AddEntry("Blog.ReadMore", "Číst více", "Read more", "Read more button text");
            AddEntry("Blog.CreatedOn", "Vytvořeno: {0}", "Created on: {0}", "Blog post creation date with date placeholder");
            AddEntry("Blog.Tags", "Štítky:", "Tags:", "Tags label");
            
            // Nové překlady pro blog
            AddEntry("Blog.Sidebar.PopularTags", "Populární štítky", "Popular Tags", "Popular tags section title in sidebar");
            AddEntry("Blog.Sidebar.Archive", "Archiv", "Archive", "Archive section title in sidebar");
            
            // --- BlogPost Seed Data ---
            AddEntry("BlogPost.Title", "Blog Příspěvek", "Blog Post", "Blog post page title");
            AddEntry("BlogPost.Loading", "Načítání příspěvku...", "Loading post...", "Blog post loading message");
            AddEntry("BlogPost.NotFound", "Příspěvek nebyl nalezen.", "Post not found.", "Blog post not found message");
            AddEntry("BlogPost.Error", "Při načítání příspěvku došlo k chybě.", "An error occurred while loading the post.", "Blog post error message");
            AddEntry("BlogPost.CreatedOn", "Vytvořeno: {0}", "Created on: {0}", "Blog post creation date with date placeholder");
            AddEntry("BlogPost.Tags", "Štítky:", "Tags:", "Tags label in blog post detail");
            AddEntry("BlogPost.BackToBlog", "Zpět na blog", "Back to blog", "Back to blog button text");
            
            // Nové překlady pro blogpost
            AddEntry("BlogPost.Share", "Sdílet", "Share", "Share button text for blog post");
            AddEntry("BlogPost.PopularTags", "Populární štítky", "Popular Tags", "Popular tags section title in blog post");
            AddEntry("BlogPost.RelatedPosts", "Související příspěvky", "Related Posts", "Related posts section title in blog post");
            // --- End Seed Data --- 

            try
            {
                // Načtení existujících klíčů z databáze
                var existingDbKeys = await context.LocalizationStrings
                    .Select(ls => new { ls.Key, ls.LanguageCode })
                    .ToListAsync();

                // Filtrování pouze nových lokalizačních klíčů
                var newEntries = localizationStrings
                    .Where(newItem => !existingDbKeys.Any(dbItem => 
                        dbItem.Key == newItem.Key && 
                        dbItem.LanguageCode == newItem.LanguageCode))
                    .ToList();

                // Přidání pouze nových klíčů do databáze
                if (newEntries.Any())
                {
                    await context.LocalizationStrings.AddRangeAsync(newEntries);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Logování chyby
                Console.WriteLine($"Chyba při seedování lokalizačních dat: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Vnitřní chyba: {ex.InnerException.Message}");
                }
                throw; // Předání chyby dál pro zpracování
            }
        }
    }
} 