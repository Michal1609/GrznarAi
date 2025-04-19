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
            AddEntry("Blog.Comment.ReCaptchaFailed", "Ověření, že nejste robot, se nezdařilo. Zkuste to prosím znovu.", "Verification that you are not a robot failed. Please try again.", "Error message when reCAPTCHA verification fails");
            AddEntry("Blog.ErrorVoting", "Chyba při hlasování. Zkuste to prosím znovu.", "Error voting. Please try again.", "Error message when blog voting fails");
            
            // --- Admin Seed Data --- 
            AddEntry("NavMenu.Admin.Administration", "Administrace", "Administration", "NavMenu Admin Link - Administration");
            AddEntry("Administration.Title", "Administrace", "Administration", "Main administration page title");
            AddEntry("Administration.Description", "Komplexní administrační rozhraní pro správu webu", "Comprehensive administration interface for website management", "Administration description");
            
            // Users & Roles management
            AddEntry("Administration.Users.Title", "Správa uživatelů", "User Management", "User management section title");
            AddEntry("Administration.Users.Description", "Správa uživatelských účtů a oprávnění", "Manage user accounts and permissions", "User management description");
            AddEntry("Administration.Users.List", "Seznam uživatelů", "User List", "User list title");
            AddEntry("Administration.Users.Create", "Vytvořit uživatele", "Create User", "Create user button");
            AddEntry("Administration.Users.Edit", "Upravit uživatele", "Edit User", "Edit user button");
            AddEntry("Administration.Users.Delete", "Smazat uživatele", "Delete User", "Delete user button");
            AddEntry("Administration.Users.ConfirmDelete", "Opravdu chcete smazat tohoto uživatele?", "Are you sure you want to delete this user?", "Confirm user deletion");
            AddEntry("Administration.Users.Email", "Email", "Email", "User email field");
            AddEntry("Administration.Users.Username", "Uživatelské jméno", "Username", "Username field");
            AddEntry("Administration.Users.Roles", "Role", "Roles", "User roles field");
            AddEntry("Administration.Users.EmailConfirmed", "Email potvrzen", "Email Confirmed", "Email confirmed field");
            AddEntry("Administration.Users.LockoutEnabled", "Uzamčení povoleno", "Lockout Enabled", "Lockout enabled field");
            AddEntry("Administration.Users.LockoutEnd", "Konec uzamčení", "Lockout End", "Lockout end field");
            AddEntry("Administration.Users.TwoFactorEnabled", "Dvoufaktorové ověření", "Two-Factor Enabled", "Two-factor authentication field");
            AddEntry("Administration.Users.AccessFailedCount", "Počet neúspěšných přihlášení", "Access Failed Count", "Failed login attempts field");
            AddEntry("Administration.Users.Filter", "Filtrovat uživatele...", "Filter users...", "User filter placeholder");
            AddEntry("Administration.Users.NoUsersFound", "Žádní uživatelé nenalezeni", "No users found", "No users found message");
            AddEntry("Administration.Users.ManageRoles", "Správa rolí", "Manage Roles", "Manage roles button");
            AddEntry("Administration.Users.Password", "Heslo", "Password", "User password field");
            AddEntry("Administration.Users.PasswordRequirements", "Heslo musí obsahovat velké písmeno, malé písmeno, číslici a speciální znak.", "Password must contain an uppercase letter, lowercase letter, digit, and special character.", "Password requirements explanation");
            AddEntry("Administration.Users.ConfirmPassword", "Potvrzení hesla", "Confirm Password", "Confirm password field");

            AddEntry("Administration.Roles.Title", "Správa rolí", "Role Management", "Role management section title");
            AddEntry("Administration.Roles.Description", "Správa uživatelských rolí a oprávnění", "Manage user roles and permissions", "Role management description");
            AddEntry("Administration.Roles.List", "Seznam rolí", "Role List", "Role list title");
            AddEntry("Administration.Roles.Create", "Vytvořit roli", "Create Role", "Create role button");
            AddEntry("Administration.Roles.Edit", "Upravit roli", "Edit Role", "Edit role button");
            AddEntry("Administration.Roles.Delete", "Smazat roli", "Delete Role", "Delete role button");
            AddEntry("Administration.Roles.ConfirmDelete", "Opravdu chcete smazat tuto roli?", "Are you sure you want to delete this role?", "Confirm role deletion");
            AddEntry("Administration.Roles.Name", "Název", "Name", "Role name field");
            AddEntry("Administration.Roles.NormalizedName", "Normalizovaný název", "Normalized Name", "Normalized role name field");
            AddEntry("Administration.Roles.Filter", "Filtrovat role...", "Filter roles...", "Role filter placeholder");
            AddEntry("Administration.Roles.NoRolesFound", "Žádné role nenalezeny", "No roles found", "No roles found message");
            AddEntry("Administration.Roles.AssignUsers", "Přiřadit uživatele", "Assign Users", "Assign users button");
            AddEntry("Administration.Roles.UserAssignment", "Přiřazení uživatelů k roli", "User Assignment", "User assignment title");
            
            // Common Admin UI elements
            AddEntry("Administration.Save", "Uložit", "Save", "Save button text");
            AddEntry("Administration.Cancel", "Zrušit", "Cancel", "Cancel button text");
            AddEntry("Administration.Back", "Zpět", "Back", "Back button text");
            AddEntry("Administration.Actions", "Akce", "Actions", "Actions column header");
            AddEntry("Administration.Filter", "Filtrovat", "Filter", "Filter button text");
            AddEntry("Administration.Search", "Vyhledat", "Search", "Search button text");
            AddEntry("Administration.Reset", "Reset", "Reset", "Reset button text");
            AddEntry("Administration.Saving", "Ukládání...", "Saving...", "Saving indicator text");
            AddEntry("Administration.Success", "Úspěch", "Success", "Success message header");
            AddEntry("Administration.Error", "Chyba", "Error", "Error message header");
            AddEntry("Administration.Warning", "Varování", "Warning", "Warning message header");
            AddEntry("Administration.Loading", "Načítání...", "Loading...", "Loading indicator text");
            AddEntry("Administration.NoData", "Žádná data", "No data", "No data message");
            AddEntry("Administration.Confirmation", "Potvrzení", "Confirmation", "Confirmation dialog title");
            AddEntry("Administration.Yes", "Ano", "Yes", "Yes button text");
            AddEntry("Administration.No", "Ne", "No", "No button text");

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