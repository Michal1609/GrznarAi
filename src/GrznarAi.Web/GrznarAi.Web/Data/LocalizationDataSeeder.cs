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

            // --- Notes Page Seed Data ---
            AddEntry("Notes.Title", "Poznámky", "Notes", "Notes page title");
            AddEntry("Notes.Categories", "Kategorie", "Categories", "Notes categories section title");
            AddEntry("Notes.AllNotes", "Všechny poznámky", "All Notes", "All notes category label");
            AddEntry("Notes.Loading", "Načítání...", "Loading...", "Notes loading message");
            AddEntry("Notes.NoNotes", "Nemáte zatím žádné poznámky", "You don't have any notes yet", "Message when no notes found");
            AddEntry("Notes.NoCategories", "Nemáte zatím žádné kategorie", "You don't have any categories yet", "Message when no categories found");
            AddEntry("Notes.Search", "Hledat v poznámkách...", "Search notes...", "Search input placeholder");
            AddEntry("Notes.New", "Nová poznámka", "New Note", "New note button");
            AddEntry("Notes.CreateFirst", "Vytvořit první poznámku", "Create first note", "Create first note button");
            AddEntry("Notes.Edit", "Upravit", "Edit", "Edit note button");
            AddEntry("Notes.Delete", "Smazat", "Delete", "Delete note button");
            AddEntry("Notes.DeleteConfirmation", "Opravdu chcete smazat tuto poznámku?", "Are you sure you want to delete this note?", "Delete note confirmation message");
            AddEntry("Notes.Deleted", "Poznámka byla smazána", "Note has been deleted", "Note deleted confirmation");
            AddEntry("Notes.ManageCategories", "Správa kategorií", "Manage Categories", "Manage note categories button");
            AddEntry("Notes.AddNew", "Přidat novou poznámku", "Add New Note", "Add new note modal title");
            AddEntry("Notes.Content", "Obsah", "Content", "Note content label");
            AddEntry("Notes.Save", "Uložit", "Save", "Save button text");
            AddEntry("Notes.Cancel", "Zrušit", "Cancel", "Cancel button text");
            AddEntry("Notes.Created", "Poznámka byla vytvořena", "Note has been created", "Note created confirmation");
            AddEntry("Notes.Updated", "Poznámka byla aktualizována", "Note has been updated", "Note updated confirmation");
            AddEntry("Notes.Error", "Nastala chyba při zpracování požadavku", "An error occurred while processing your request", "Error message");
            AddEntry("Notes.TitleRequired", "Název poznámky je povinný", "Note title is required", "Title required validation message");
            AddEntry("Notes.Notification", "Oznámení", "Notification", "Alert notification title");
            AddEntry("Notes.Done", "Hotovo", "Done", "Done button text");

            // Notes categories related
            AddEntry("Notes.Categories.AddNew", "Přidat novou kategorii", "Add New Category", "Add new category modal title");
            AddEntry("Notes.Categories.Edit", "Upravit kategorii", "Edit Category", "Edit category modal title");
            AddEntry("Notes.Categories.Name", "Název", "Name", "Category name label");
            AddEntry("Notes.Categories.Description", "Popis", "Description", "Category description label");
            AddEntry("Notes.Categories.NameRequired", "Název kategorie je povinný", "Category name is required", "Category name required validation message");
            AddEntry("Notes.Categories.Created", "Kategorie byla vytvořena", "Category has been created", "Category created confirmation");
            AddEntry("Notes.Categories.Updated", "Kategorie byla aktualizována", "Category has been updated", "Category updated confirmation");
            AddEntry("Notes.Categories.Deleted", "Kategorie byla smazána", "Category has been deleted", "Category deleted confirmation");
            AddEntry("Notes.Categories.DeleteConfirmation", "Opravdu chcete smazat tuto kategorii? Poznámky v této kategorii nebudou smazány.", "Are you sure you want to delete this category? Notes in this category will not be deleted.", "Delete category confirmation message");
            
            // NavMenu entry for Notes
            AddEntry("NavMenu.Notes", "Poznámky", "Notes", "NavMenu Link - Notes");

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