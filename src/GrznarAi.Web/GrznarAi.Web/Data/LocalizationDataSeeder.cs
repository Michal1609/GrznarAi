using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
            AddEntry("Blog.NoPostsFoundInLanguage", "Nebyly nalezeny žádné příspěvky v aktuálním jazyce. Zobrazuji příspěvky v jiných jazycích.", "No posts found in current language. Showing posts in other languages.", "Message when no blog posts are found in current language");
            AddEntry("Blog.SearchResults", "Výsledky vyhledávání pro: {0}", "Search results for: {0}", "Search results message with search term placeholder");
            AddEntry("Blog.ClearSearch", "Vymazat vyhledávání", "Clear search", "Clear search button text");
            AddEntry("Blog.Search.Placeholder", "Hledat v blogu...", "Search blog...", "Search input placeholder");
            AddEntry("Blog.Search.Button", "Hledat", "Search", "Search button text");
            AddEntry("Blog.PopularTags", "Populární štítky", "Popular Tags", "Popular tags section title");
            AddEntry("Blog.Archive", "Archiv", "Archive", "Archive section title");
            AddEntry("Blog.ReadMore", "Číst více", "Read more", "Read more button text");
            AddEntry("Blog.CreatedOn", "Vytvořeno: {0}", "Created on: {0}", "Blog post creation date with date placeholder");
            AddEntry("Blog.Tags", "Štítky:", "Tags:", "Tags label");
            AddEntry("Blog.NoTags", "Žádné štítky", "No tags", "Message when blog post has no tags");
            AddEntry("Blog.Pagination.Next", "Další", "Next", "Next page button text");
            AddEntry("Blog.Pagination.Previous", "Předchozí", "Previous", "Previous page button text");
            AddEntry("Blog.Pagination.Page", "Strana {0}", "Page {0}", "Page number indicator");
            AddEntry("Blog.Pagination.Of", "z {0}", "of {0}", "Page count indicator");
            AddEntry("Blog.Date.January", "Leden", "January", "Month name - January");
            AddEntry("Blog.Date.February", "Únor", "February", "Month name - February");
            AddEntry("Blog.Date.March", "Březen", "March", "Month name - March");
            AddEntry("Blog.Date.April", "Duben", "April", "Month name - April");
            AddEntry("Blog.Date.May", "Květen", "May", "Month name - May");
            AddEntry("Blog.Date.June", "Červen", "June", "Month name - June");
            AddEntry("Blog.Date.July", "Červenec", "July", "Month name - July");
            AddEntry("Blog.Date.August", "Srpen", "August", "Month name - August");
            AddEntry("Blog.Date.September", "Září", "September", "Month name - September");
            AddEntry("Blog.Date.October", "Říjen", "October", "Month name - October");
            AddEntry("Blog.Date.November", "Listopad", "November", "Month name - November");
            AddEntry("Blog.Date.December", "Prosinec", "December", "Month name - December");
            
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
            AddEntry("BlogPost.ShareTitle", "Podívejte se na tento blog", "Check out this blog post", "Title for blog sharing");
            AddEntry("BlogPost.ShareText", "Myslím, že by vás mohl zajímat tento blog", "I thought you might be interested in this blog post", "Text for blog sharing");
            
            // Komentáře a hlasování
            AddEntry("Blog.Comments", "Komentáře", "Comments", "Comments section title");
            AddEntry("Blog.AddComment", "Přidat komentář", "Add a comment", "Add comment button text");
            AddEntry("Blog.NoComments", "Zatím žádné komentáře. Buďte první!", "No comments yet. Be the first to comment!", "Message shown when no comments exist");
            AddEntry("Blog.LoadMoreComments", "Načíst další komentáře", "Load more comments", "Button to load more comments");
            AddEntry("Blog.LeaveComment", "Napsat komentář", "Leave a comment", "Title for comment form");
            AddEntry("Blog.LeaveReply", "Odpovědět", "Leave a reply", "Title for reply form");
            AddEntry("Blog.Comment.Name", "Jméno", "Name", "Name label in comment form");
            AddEntry("Blog.Comment.Email", "E-mail", "Email", "Email label in comment form");
            AddEntry("Blog.Comment.EmailOptional", "E-mail (nepovinné)", "Email (optional)", "Optional email placeholder");
            AddEntry("Blog.Comment.Content", "Váš komentář", "Your comment", "Comment content label");
            AddEntry("Blog.Comment.Submit", "Odeslat", "Submit", "Submit button text for comment form");
            AddEntry("Blog.Comment.Cancel", "Zrušit", "Cancel", "Cancel button for comment form");
            AddEntry("Blog.Comment.Saving", "Ukládání...", "Saving...", "Saving message for comment form");
            AddEntry("Blog.Comment.Edit", "Upravit", "Edit", "Edit comment button text");
            AddEntry("Blog.Comment.Delete", "Smazat", "Delete", "Delete comment button text");
            AddEntry("Blog.Comment.Reply", "Odpovědět", "Reply", "Reply to comment button text");
            AddEntry("Blog.Comment.Save", "Uložit", "Save", "Save edited comment button text");
            AddEntry("Blog.Comment.ErrorSaving", "Chyba při ukládání komentáře. Zkuste to prosím znovu.", "Error saving comment. Please try again.", "Error message when saving comment fails");
            AddEntry("Blog.Comment.ErrorUpdating", "Chyba při aktualizaci komentáře. Zkuste to prosím znovu.", "Error updating comment. Please try again.", "Error message when updating comment fails");
            AddEntry("Blog.Comment.ErrorDeleting", "Chyba při mazání komentáře. Zkuste to prosím znovu.", "Error deleting comment. Please try again.", "Error message when deleting comment fails");
            AddEntry("Blog.Comment.ConfirmDelete", "Opravdu chcete smazat tento komentář?", "Are you sure you want to delete this comment?", "Confirmation message for comment deletion");
            AddEntry("Blog.Comment.ErrorVoting", "Chyba při hlasování. Zkuste to prosím znovu.", "Error voting. Please try again.", "Error message when voting fails");
            AddEntry("Blog.ErrorVoting", "Chyba při hlasování. Zkuste to prosím znovu.", "Error voting. Please try again.", "Error message when blog voting fails");
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