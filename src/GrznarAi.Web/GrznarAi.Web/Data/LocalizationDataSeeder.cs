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
            AddEntry("HomePage.LatestPosts.NoPosts", "Zatím zde není žádný příspěvek", "No posts righ now", "No posts righ now");

            // --- Contact Page Seed Data ---
            AddEntry("ContactPage.Title", "Kontakt - GrznarAI", "Contact - GrznarAI", "Contact page title");
            AddEntry("ContactPage.Heading", "Kontaktujte nás", "Contact Us", "Contact page heading");
            
            // Contact cards
            AddEntry("ContactPage.Email.Title", "Email", "Email", "Contact card title - Email");
            AddEntry("ContactPage.Email.SendButton", "Odeslat email", "Send Email", "Contact card button - Email");
            AddEntry("ContactPage.GitHub.Title", "GitHub", "GitHub", "Contact card title - GitHub");
            AddEntry("ContactPage.GitHub.ViewButton", "Zobrazit profil", "View Profile", "Contact card button - GitHub");
            AddEntry("ContactPage.LinkedIn.Title", "LinkedIn", "LinkedIn", "Contact card title - LinkedIn");
            AddEntry("ContactPage.LinkedIn.ConnectButton", "Spojit se", "Connect", "Contact card button - LinkedIn");
            
            // Contact form
            AddEntry("ContactPage.Form.Title", "Pošlete nám zprávu", "Send Us a Message", "Contact form title");
            AddEntry("ContactPage.Form.SuccessMessage", "Vaše zpráva byla úspěšně odeslána. Děkujeme!", "Your message has been successfully sent. Thank you!", "Contact form success message");
            AddEntry("ContactPage.Form.ErrorMessage", "Při odesílání zprávy došlo k chybě.", "An error occurred while sending the message.", "Contact form error message");
            AddEntry("ContactPage.Form.RecaptchaFailed", "Ověření, že nejste robot, se nezdařilo. Zkuste to prosím znovu.", "Verification that you are not a robot failed. Please try again.", "reCAPTCHA verification failed message");
            AddEntry("ContactPage.Form.RecaptchaText", "Tento formulář je chráněn pomocí Google reCAPTCHA.", "This form is protected by Google reCAPTCHA.", "reCAPTCHA informational text");
            AddEntry("ContactPage.Form.NameLabel", "Vaše jméno", "Your Name", "Contact form name label");
            AddEntry("ContactPage.Form.NamePlaceholder", "Zadejte své jméno", "Enter your name", "Contact form name placeholder");
            AddEntry("ContactPage.Form.EmailLabel", "E-mailová adresa", "Email Address", "Contact form email label");
            AddEntry("ContactPage.Form.EmailPlaceholder", "Zadejte svůj e-mail", "Enter your email", "Contact form email placeholder");
            AddEntry("ContactPage.Form.SubjectLabel", "Předmět", "Subject", "Contact form subject label");
            AddEntry("ContactPage.Form.SubjectPlaceholder", "Zadejte předmět", "Enter subject", "Contact form subject placeholder");
            AddEntry("ContactPage.Form.MessageLabel", "Zpráva", "Message", "Contact form message label");
            AddEntry("ContactPage.Form.MessagePlaceholder", "Zadejte vaši zprávu", "Enter your message", "Contact form message placeholder");
            AddEntry("ContactPage.Form.SendButton", "Odeslat zprávu", "Send Message", "Contact form send button");
            AddEntry("ContactPage.Form.SendingText", "Odesílání...", "Sending...", "Contact form sending text");
            
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
            
            // --- Applications Menu Seed Data ---
            AddEntry("NavMenu.Applications", "Aplikace", "Applications", "NavMenu Applications Dropdown");
            AddEntry("NavMenu.Applications.Notes", "Poznámky", "Notes", "NavMenu Applications Link - Notes");

            // --- Notes Application Seed Data ---
            AddEntry("Notes.Title", "Poznámky", "Notes", "Notes page title");
            AddEntry("Notes.Description", "Osobní poznámky", "Personal notes", "Notes page description");
            AddEntry("Notes.NoNotes", "Zatím nemáte žádné poznámky", "You don't have any notes yet", "Message when user has no notes");
            AddEntry("Notes.CreateNote", "Nová poznámka", "New note", "Button to create a new note");
            AddEntry("Notes.EditNote", "Upravit poznámku", "Edit note", "Button to edit a note");
            AddEntry("Notes.DeleteNote", "Smazat poznámku", "Delete note", "Button to delete a note");
            AddEntry("Notes.Search", "Vyhledat poznámky", "Search notes", "Search placeholder");
            AddEntry("Notes.Categories", "Kategorie", "Categories", "Categories section title");
            AddEntry("Notes.NoCategories", "Zatím nemáte žádné kategorie", "You don't have any categories yet", "Message when user has no categories");
            AddEntry("Notes.CreateCategory", "Nová kategorie", "New category", "Button to create a new category");
            AddEntry("Notes.EditCategory", "Upravit kategorii", "Edit category", "Button to edit a category");
            AddEntry("Notes.DeleteCategory", "Smazat kategorii", "Delete category", "Button to delete a category");
            AddEntry("Notes.AddToCategory", "Přidat do kategorie", "Add to category", "Button to add note to category");
            AddEntry("Notes.RemoveFromCategory", "Odebrat z kategorie", "Remove from category", "Button to remove note from category");
            AddEntry("Notes.SaveNote", "Uložit poznámku", "Save note", "Button to save a note");
            AddEntry("Notes.SaveCategory", "Uložit kategorii", "Save category", "Button to save a category");
            AddEntry("Notes.Cancel", "Zrušit", "Cancel", "Button to cancel an action");
            AddEntry("Notes.SearchResults", "Výsledky vyhledávání", "Search results", "Search results title");
            AddEntry("Notes.Title.Label", "Název", "Title", "Note title label");
            AddEntry("Notes.Content.Label", "Obsah", "Content", "Note content label");
            AddEntry("Notes.Category.Label", "Název kategorie", "Category name", "Category name label");
            AddEntry("Notes.Category.Description", "Popis", "Description", "Category description label");
            AddEntry("Notes.ConfirmDelete", "Opravdu chcete smazat tuto položku?", "Are you sure you want to delete this item?", "Confirmation message for deletion");
            AddEntry("Notes.Created", "Vytvořeno", "Created", "Created date label");
            AddEntry("Notes.Updated", "Aktualizováno", "Updated", "Updated date label");
            AddEntry("Notes.NoPermission", "Nemáte oprávnění pro používání aplikace Poznámky", "You don't have permission to use the Notes application", "No permission message");

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
            AddEntry("Blog.NoResults", "Žádné příspěvky", "No posts", "No posts");
            AddEntry("Blog.NoResults.Description", "Zatím zde není žádný příspvěk", "No posts now", "No post now");
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
            
            AddEntry("Administration.Blogs.Title", "Správa blogu", "Blog administration", "Blog administrationtitle");
            AddEntry("Administration.Blogs.Description", "Správa blogu", "Blog administration", "Blog administrationtitle");

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

            // --- AI News Page Seed Data ---
            AddEntry("NavMenu.AiNews", "AI Novinky", "AI News", "NavMenu Link - AI News");
            AddEntry("AiNews.Title", "AI Novinky", "AI News", "AI News page title");
            AddEntry("AiNews.Description", "Nejnovější zprávy ze světa umělé inteligence", "Latest news from the world of artificial intelligence", "AI News page description");
            AddEntry("AiNews.Loading", "Načítání novinek...", "Loading news...", "AI News loading indicator");
            AddEntry("AiNews.NoNews", "Žádné novinky k zobrazení", "No news to display", "AI News empty state");
            AddEntry("AiNews.ReadMore", "Číst více", "Read more", "AI News read more button");
            AddEntry("AiNews.FilteringByDate", "Filtrování podle data", "Filtering by date", "AI News date filter info");
            AddEntry("AiNews.SearchPlaceholder", "Vyhledávání v novinkách...", "Search news...", "AI News search placeholder");
            AddEntry("AiNews.Search", "Hledat", "Search", "AI News search button text");
            AddEntry("AiNews.ArchiveTitle", "Archiv", "Archive", "AI News archive title");
            AddEntry("AiNews.ReadOriginal", "Číst originál", "Read original", "AI News read original button");
            
            // --- Administration AI News --- 
            AddEntry("Administration.AiNews.Title", "Správa AI novinek", "AI News Management", "Admin AI News page title");
            AddEntry("Administration.AiNews.Description", "Přidávání, úprava a mazání novinek ze světa umělé inteligence", "Add, edit and delete news from the world of artificial intelligence", "Admin AI News page description");
            AddEntry("Administration.AiNews.List", "Seznam AI novinek", "AI News List", "AI News administration list title");
            AddEntry("Administration.AiNews.Filter", "Filtrovat podle titulku nebo obsahu", "Filter by title or content", "AI News administration filter placeholder");
            AddEntry("Administration.AiNews.Delete", "Smazat", "Delete", "AI News administration delete button");
            AddEntry("Administration.AiNews.DeleteConfirmation", "Opravdu chcete smazat tuto novinku?", "Are you sure you want to delete this news item?", "AI News administration delete confirmation");
            AddEntry("Administration.AiNews.NoItems", "Žádné AI novinky nenalezeny", "No AI news found", "AI News administration no items message");
            
            // Nové lokalizační řetězce pro správu zdrojů AI novinek
            AddEntry("Administration.AiNews.Sources", "Zdroje AI novinek", "AI News Sources", "AI News sources tab title");
            AddEntry("Administration.AiNews.Sources.Add", "Přidat nový zdroj", "Add New Source", "Add new source button text");
            AddEntry("Administration.AiNews.Sources.Edit", "Upravit zdroj", "Edit Source", "Edit source button text");
            AddEntry("Administration.AiNews.Sources.Delete", "Smazat zdroj", "Delete Source", "Delete source button text");
            AddEntry("Administration.AiNews.Sources.DeleteConfirmation", "Opravdu chcete smazat tento zdroj novinek?", "Are you sure you want to delete this news source?", "Delete source confirmation message");
            AddEntry("Administration.AiNews.Sources.DeleteWarning", "Pozor: Smazáním zdroje budou ovlivněny všechny novinky z tohoto zdroje.", "Warning: Deleting this source will affect all news from this source.", "Delete source warning message");
            AddEntry("Administration.AiNews.Sources.NoItems", "Žádné zdroje novinek nebyly nalezeny.", "No news sources found.", "No sources message");
            
            // Lokalizační řetězce pro formulář zdroje
            AddEntry("Administration.AiNews.Sources.Name", "Název zdroje", "Source Name", "Source name field label");
            AddEntry("Administration.AiNews.Sources.Url", "URL zdroje", "Source URL", "Source URL field label");
            AddEntry("Administration.AiNews.Sources.Type", "Typ zdroje", "Source Type", "Source type field label");
            AddEntry("Administration.AiNews.Sources.Active", "Aktivní", "Active", "Source active field label");
            AddEntry("Administration.AiNews.Sources.Description", "Popis", "Description", "Source description field label");
            AddEntry("Administration.AiNews.Sources.Parameters", "Parametry (JSON)", "Parameters (JSON)", "Source parameters field label");
            AddEntry("Administration.AiNews.Sources.ParametersHelp", "Volitelná konfigurace specifická pro tento zdroj v JSON formátu.", "Optional source-specific configuration in JSON format.", "Source parameters help text");
            
            // Lokalizační řetězce pro chyby při stahování
            AddEntry("Administration.AiNews.Errors", "Chyby při stahování", "Download Errors", "AI News errors tab title");
            AddEntry("Administration.AiNews.Errors.Delete", "Smazat záznam", "Delete Record", "Delete error record button text");
            AddEntry("Administration.AiNews.Errors.DeleteConfirmation", "Opravdu chcete smazat tento záznam o chybě?", "Are you sure you want to delete this error record?", "Delete error confirmation message");
            AddEntry("Administration.AiNews.Errors.NoItems", "Žádné chyby při stahování novinek nebyly zaznamenány.", "No download errors were recorded.", "No errors message");
            AddEntry("Administration.AiNews.Errors.ShowDetails", "Zobrazit detail", "Show Details", "Show error details button");
            AddEntry("Administration.AiNews.Errors.StackTrace", "Stack Trace:", "Stack Trace:", "Stack trace label");
            AddEntry("Administration.AiNews.Errors.NoStackTrace", "Stack trace není k dispozici", "Stack trace not available", "No stack trace message");

            // --- Permissions Management Section ---
            AddEntry("Administration.Permissions.Title", "Správa oprávnění", "Permission Management", "Permission management section title");
            AddEntry("Administration.Permissions.Description", "Správa uživatelských oprávnění pro aplikace", "Manage user application permissions", "Permission management description");
            AddEntry("Administration.Permissions.List", "Seznam oprávnění", "Permission List", "Permission list title");
            AddEntry("Administration.Permissions.AssignPermission", "Přiřadit oprávnění", "Assign Permission", "Assign permission button");
            AddEntry("Administration.Permissions.RemovePermission", "Odebrat oprávnění", "Remove Permission", "Remove permission button");
            AddEntry("Administration.Permissions.SelectUser", "Vyberte uživatele", "Select User", "User selection label");
            AddEntry("Administration.Permissions.SelectPermission", "Vyberte oprávnění", "Select Permission", "Permission selection label");
            AddEntry("Administration.Permissions.UserPermissions", "Oprávnění uživatele", "User Permissions", "User permissions section title");
            AddEntry("Administration.Permissions.NoPermissions", "Uživatel nemá žádná oprávnění", "User has no permissions", "No permissions message");
            
            // --- Blog Management Section ---
            AddEntry("Administration.Blogs.Title", "Správa blogu", "Blog administration", "Blog administrationtitle");
            AddEntry("Administration.Blogs.Description", "Správa blogu", "Blog administration", "Blog administrationtitle");

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