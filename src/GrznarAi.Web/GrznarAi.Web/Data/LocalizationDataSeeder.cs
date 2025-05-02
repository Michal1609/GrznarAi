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
            AddEntry("NavMenu.Home", "Úvod", "Home", "NavMenu Link - Home");
            AddEntry("NavMenu.About", "O mně", "About", "NavMenu Link - About");
            AddEntry("NavMenu.Projects", "Projekty", "Projects", "NavMenu Link - Projects");
            AddEntry("NavMenu.Blog", "Blog", "Blog", "NavMenu Link - Blog");
            AddEntry("NavMenu.AiNews", "AI Novinky", "AI News", "NavMenu Link - AiNews");
            AddEntry("NavMenu.Contact", "Kontakt", "Contact", "NavMenu Link - Contact");
            AddEntry("NavMenu.MeteoStation", "Meteostanice", "Weather Station", "NavMenu Link - Weather Station");
            AddEntry("NavMenu.Meteo", "Meteostanice", "Weather Station", "NavMenu Link - Meteo");
            
            // Applications section
            AddEntry("NavMenu.Applications", "Aplikace", "Applications", "NavMenu Applications section heading");
            AddEntry("NavMenu.Applications.Notes", "Poznámky", "Notes", "NavMenu Link - Notes under Applications");

            // Admin section
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
            AddEntry("Blog.Sidebar.PopularTags", "Populární štítky", "Popular Tags", "Sidebar popular tags heading");
            AddEntry("Blog.Sidebar.NoTags", "Žádné štítky nebyly nalezeny", "No tags found", "Message when no tags are found in sidebar");
            AddEntry("Blog.Sidebar.Archive", "Archiv", "Archive", "Sidebar archive heading");
            AddEntry("Blog.Sidebar.NoArchive", "Žádné archivy nebyly nalezeny", "No archives found", "Message when no archives are found in sidebar");
            AddEntry("Blog.Archive", "Archiv", "Archive", "Archive section title");
            AddEntry("Blog.NoArchives", "Žádné archivy", "No archives", "Message when no archives are found");
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

            // --- Meteo Page Localization ---
            AddEntry("Meteo.Title", "Meteostanice", "Weather Station", "Meteo page title");
            AddEntry("Meteo.Description", "Tato stránka zobrazuje data z osobní meteostanice v reálném čase. Data jsou pravidelně aktualizována.", "This page displays real-time data from my personal weather station. The data is regularly updated.", "Meteo page description");
            AddEntry("Meteo.CurrentWeather", "Aktuální počasí", "Current Weather", "Current weather section title");
            AddEntry("Meteo.IndoorConditions", "Vnitřní podmínky", "Indoor Conditions", "Indoor conditions section title");
            AddEntry("Meteo.Precipitation", "Srážky", "Precipitation", "Precipitation section title");
            AddEntry("Meteo.SolarRadiation", "Sluneční záření", "Solar Radiation", "Solar radiation section title");
            AddEntry("Meteo.Loading", "Načítání...", "Loading...", "Loading text");
            AddEntry("Meteo.Error", "Nepodařilo se načíst data z meteostanice. Zkuste to prosím později.", "Failed to load weather station data. Please try again later.", "Error message");
            AddEntry("Meteo.LastUpdated", "Poslední aktualizace", "Last updated", "Last updated label");
            AddEntry("Meteo.Temperature", "Teplota", "Temperature", "Temperature label");
            AddEntry("Meteo.Humidity", "Vlhkost", "Humidity", "Humidity label");
            AddEntry("Meteo.Pressure", "Tlak", "Pressure", "Pressure label");
            AddEntry("Meteo.WindSpeed", "Rychlost větru", "Wind Speed", "Wind speed label");
            AddEntry("Meteo.WindDirection", "Směr větru", "Wind Direction", "Wind direction label");
            AddEntry("Meteo.IndoorTemperature", "Vnitřní teplota", "Indoor Temperature", "Indoor temperature label");
            AddEntry("Meteo.IndoorHumidity", "Vnitřní vlhkost", "Indoor Humidity", "Indoor humidity label");
            AddEntry("Meteo.FeelsLike", "Pocitová teplota", "Feels Like", "Feels like temperature label");
            AddEntry("Meteo.Unknown", "Neznámý", "Unknown", "Unknown value");
            AddEntry("Meteo.RainfallRate", "Intenzita srážek", "Rainfall Rate", "Rainfall rate label");
            AddEntry("Meteo.RainfallDaily", "Denní srážky", "Daily Rainfall", "Daily rainfall label");
            AddEntry("Meteo.RainfallWeekly", "Týdenní srážky", "Weekly Rainfall", "Weekly rainfall label");
            AddEntry("Meteo.RainfallMonthly", "Měsíční srážky", "Monthly Rainfall", "Monthly rainfall label");
            AddEntry("Meteo.SolarRadiation", "Sluneční záření", "Solar Radiation", "Solar radiation label");
            AddEntry("Meteo.UVIndex", "UV index", "UV Index", "UV Index label");
            AddEntry("Meteo.IndexUnit", "index", "index", "Index unit");
            AddEntry("Meteo.DewPoint", "Rosný bod", "Dew Point", "Dew point label");
            AddEntry("Meteo.WindGust", "Poryvy větru", "Wind Gust", "Wind gust label");
            AddEntry("Meteo.RefreshData", "Aktualizovat data", "Refresh Data", "Refresh data button");

            // Weather types
            AddEntry("Meteo.Weather.Rainy", "Deštivo", "Rainy", "Rainy weather type");
            AddEntry("Meteo.Weather.Snowy", "Sněžení", "Snowy", "Snowy weather type");
            AddEntry("Meteo.Weather.Sunny", "Slunečno", "Sunny", "Sunny weather type");
            AddEntry("Meteo.Weather.Humid", "Vlhko", "Humid", "Humid weather type");
            AddEntry("Meteo.Weather.PartlyCloudy", "Polojasno", "Partly Cloudy", "Partly cloudy weather type");
            AddEntry("Meteo.Weather.Night", "Noc", "Night", "Night weather type");

            // Wind directions
            AddEntry("Meteo.WindDirection.N", "Sever", "North", "North wind direction");
            AddEntry("Meteo.WindDirection.NE", "Severovýchod", "Northeast", "Northeast wind direction");
            AddEntry("Meteo.WindDirection.E", "Východ", "East", "East wind direction");
            AddEntry("Meteo.WindDirection.SE", "Jihovýchod", "Southeast", "Southeast wind direction");
            AddEntry("Meteo.WindDirection.S", "Jih", "South", "South wind direction");
            AddEntry("Meteo.WindDirection.SW", "Jihozápad", "Southwest", "Southwest wind direction");
            AddEntry("Meteo.WindDirection.W", "Západ", "West", "West wind direction");
            AddEntry("Meteo.WindDirection.NW", "Severozápad", "Northwest", "Northwest wind direction");

            // --- AI News Localization ---
            AddEntry("AiNews.Title", "AI Novinky", "AI News", "AI News page title");
            AddEntry("AiNews.Description", "Nejnovější zprávy ze světa umělé inteligence", "Latest news from the world of artificial intelligence", "AI News page description");
            AddEntry("AiNews.SearchPlaceholder", "Vyhledat v novinkách...", "Search in news...", "Search placeholder for AI News");
            AddEntry("AiNews.Search", "Vyhledat", "Search", "Search button text");
            AddEntry("AiNews.Loading", "Načítání novinek...", "Loading news...", "Loading message for AI News");
            AddEntry("AiNews.NoNews", "Žádné novinky k zobrazení", "No news to display", "Message when no news are available");
            AddEntry("AiNews.ReadOriginal", "Číst originál", "Read original", "Button to read original article");
            AddEntry("AiNews.CzechTranslation", "Český překlad", "Czech translation", "Button to see Czech translation");
            AddEntry("AiNews.FilteringByDate", "Filtrování podle data", "Filtering by date", "Filtering by date message");
            AddEntry("AiNews.TranslatedByAI", "Přeloženo pomocí AI", "Translated by AI", "Translated by AI message");

            // --- Administration AI News Section ---
            AddEntry("Administration.AiNews.Title", "Správa AI novinek", "AI News Management", "AI News administration title");
            AddEntry("Administration.AiNews.Description", "Správa novinek o umělé inteligenci, zdrojů a chyb", "Manage artificial intelligence news, sources and errors", "AI News administration description");
            AddEntry("Administration.AiNews.List", "Seznam novinek", "News List", "News list tab");
            AddEntry("Administration.AiNews.Sources", "Zdroje", "Sources", "Sources tab");
            AddEntry("Administration.AiNews.Errors", "Chyby", "Errors", "Errors tab");
            AddEntry("Administration.AiNews.Filter", "Filtrovat novinky...", "Filter news...", "Filter placeholder");
            AddEntry("Administration.AiNews.NoItems", "Žádné novinky k zobrazení", "No news to display", "Message when no news are available");
            AddEntry("Administration.AiNews.Delete", "Smazat", "Delete", "Delete button");
            AddEntry("Administration.AiNews.DeleteConfirmation", "Opravdu chcete smazat tuto novinku?", "Are you sure you want to delete this news item?", "Delete confirmation");
            
            // AI News Sources Administration
            AddEntry("Administration.AiNews.Sources.Add", "Přidat zdroj", "Add source", "Add source button");
            AddEntry("Administration.AiNews.Sources.Edit", "Upravit zdroj", "Edit source", "Edit source button");
            AddEntry("Administration.AiNews.Sources.Delete", "Smazat zdroj", "Delete source", "Delete source button");
            AddEntry("Administration.AiNews.Sources.DeleteConfirmation", "Opravdu chcete smazat tento zdroj?", "Are you sure you want to delete this source?", "Delete source confirmation");
            AddEntry("Administration.AiNews.Sources.DeleteWarning", "Smazáním zdroje budou odstraněny všechny novinky z tohoto zdroje!", "Deleting the source will remove all news from this source!", "Delete source warning");
            AddEntry("Administration.AiNews.Sources.NoItems", "Žádné zdroje k zobrazení", "No sources to display", "No sources message");
            AddEntry("Administration.AiNews.Sources.Name", "Název zdroje", "Source name", "Source name label");
            AddEntry("Administration.AiNews.Sources.Url", "URL adresa", "URL address", "Source URL label");
            AddEntry("Administration.AiNews.Sources.Type", "Typ zdroje", "Source type", "Source type label");
            AddEntry("Administration.AiNews.Sources.Active", "Aktivní", "Active", "Active source checkbox");
            AddEntry("Administration.AiNews.Sources.Description", "Popis", "Description", "Source description label");
            AddEntry("Administration.AiNews.Sources.Parameters", "Parametry", "Parameters", "Source parameters label");
            AddEntry("Administration.AiNews.Sources.ParametersHelp", "Zadejte parametry ve formátu JSON pro rozšířené nastavení (volitelné)", "Enter parameters in JSON format for advanced configuration (optional)", "Source parameters help text");
            
            // AI News Errors Administration
            AddEntry("Administration.AiNews.Errors.NoItems", "Žádné chyby k zobrazení", "No errors to display", "No errors message");
            AddEntry("Administration.AiNews.Errors.Delete", "Smazat chybu", "Delete error", "Delete error button");
            AddEntry("Administration.AiNews.Errors.DeleteConfirmation", "Opravdu chcete smazat tento záznam o chybě?", "Are you sure you want to delete this error record?", "Delete error confirmation");
            AddEntry("Administration.AiNews.Errors.ShowDetails", "Zobrazit detaily", "Show details", "Show details button");
            AddEntry("Administration.AiNews.Errors.StackTrace", "Detaily chyby (Stack Trace)", "Error details (Stack Trace)", "Error stack trace label");
            AddEntry("Administration.AiNews.Errors.NoStackTrace", "Žádné detaily chyby k dispozici", "No error details available", "No stack trace message");
            
            // General Administration Keys
            AddEntry("Administration.Title", "Administrace", "Administration", "Administration page title");
            AddEntry("Administration.Description", "Správa systému a uživatelů", "System and user management", "Administration page description");
            AddEntry("Administration.Blogs.Title", "Správa blogu", "Blog Management", "Blog administration title");
            AddEntry("Administration.Blogs.Description", "Vytvářejte, upravujte a spravujte příspěvky na blogu", "Create, edit and manage blog posts", "Blog administration description");
            AddEntry("Administration.Users.Title", "Správa uživatelů", "User Management", "User administration title");
            AddEntry("Administration.Users.Description", "Spravujte uživatelské účty a oprávnění", "Manage user accounts and permissions", "User administration description");
            AddEntry("Administration.Roles.Title", "Správa rolí", "Role Management", "Role administration title");
            AddEntry("Administration.Roles.Description", "Definujte a spravujte uživatelské role a oprávnění", "Define and manage user roles and permissions", "Role administration description");

            // Common Admin Terms
            AddEntry("Administration.Search", "Hledat", "Search", "Admin search button");
            AddEntry("Administration.Loading", "Načítání...", "Loading...", "Admin loading message");
            AddEntry("Administration.Save", "Uložit", "Save", "Admin save button");
            AddEntry("Administration.Cancel", "Zrušit", "Cancel", "Admin cancel button");
            AddEntry("Administration.Actions", "Akce", "Actions", "Admin actions column");
            AddEntry("Administration.Confirmation", "Potvrzení", "Confirmation", "Admin confirmation dialog title");
            
            // Common Terms
            AddEntry("Common.Clear", "Vymazat", "Clear", "Clear button/filter");

            // --- Cache Admin Localization ---
            AddEntry("CacheAdmin.Title", "Správa cache", "Cache Management", "Cache admin page title");
            AddEntry("CacheAdmin.Description", "Správa systémové cache, možnost prohlížení a invalidace kešovaných dat.", "Manage system cache, view and invalidate cached data.", "Cache admin page description");
            AddEntry("CacheAdmin.Refresh", "Obnovit", "Refresh", "Refresh button");
            AddEntry("CacheAdmin.ClearAll", "Vyčistit vše", "Clear All", "Clear all button");
            AddEntry("CacheAdmin.TotalItems", "Celkem položek", "Total Items", "Total items label");
            AddEntry("CacheAdmin.TotalSize", "Celková velikost", "Total Size", "Total size label");
            AddEntry("CacheAdmin.ExpiredItems", "Expirované položky", "Expired Items", "Expired items label");
            AddEntry("CacheAdmin.SearchPlaceholder", "Hledat podle klíče nebo typu...", "Search by key or type...", "Search placeholder");
            AddEntry("CacheAdmin.Loading", "Načítání...", "Loading...", "Loading text");
            AddEntry("CacheAdmin.NoItems", "Žádné položky v cache", "No items in cache", "No items message");
            AddEntry("CacheAdmin.NoSearchResults", "Žádné výsledky pro vaše vyhledávání", "No results for your search", "No search results message");
            AddEntry("CacheAdmin.Key", "Klíč", "Key", "Key column header");
            AddEntry("CacheAdmin.Type", "Typ", "Type", "Type column header");
            AddEntry("CacheAdmin.Created", "Vytvořeno", "Created", "Created column header");
            AddEntry("CacheAdmin.Expires", "Expiruje", "Expires", "Expires column header");
            AddEntry("CacheAdmin.Size", "Velikost", "Size", "Size column header");
            AddEntry("CacheAdmin.Actions", "Akce", "Actions", "Actions column header");
            AddEntry("CacheAdmin.NoExpiration", "Bez expirace", "No expiration", "No expiration text");
            AddEntry("CacheAdmin.ConfirmClearTitle", "Vyčištění cache", "Clear Cache", "Clear cache confirmation title");
            AddEntry("CacheAdmin.ConfirmClearMessage", "Opravdu chcete vyčistit celou cache? Tato akce odstraní všechna kešovaná data.", "Are you sure you want to clear the entire cache? This action will remove all cached data.", "Clear cache confirmation message");
            AddEntry("CacheAdmin.ConfirmDeleteTitle", "Odstranit položku", "Delete Item", "Delete item confirmation title");
            AddEntry("CacheAdmin.ConfirmDeleteMessage", "Opravdu chcete odstranit tuto položku z cache?", "Are you sure you want to remove this item from cache?", "Delete item confirmation message");
            AddEntry("CacheAdmin.Cancel", "Zrušit", "Cancel", "Cancel button");
            AddEntry("CacheAdmin.Confirm", "Potvrdit", "Confirm", "Confirm button");

            // --- End of new sections ---

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

            // Přidáme lokalizační řetězce pro emailové šablony
            var emailTemplatesStrings = new List<LocalizationString>
            {
                // České lokalizační řetězce
                new LocalizationString { Key = "EmailTemplates.Title", LanguageCode = "cs", Value = "Správa emailových šablon" },
                new LocalizationString { Key = "EmailTemplates.Create", LanguageCode = "cs", Value = "Vytvořit šablonu" },
                new LocalizationString { Key = "EmailTemplates.Edit", LanguageCode = "cs", Value = "Upravit šablonu" },
                new LocalizationString { Key = "EmailTemplates.TemplateKey", LanguageCode = "cs", Value = "Klíč šablony" },
                new LocalizationString { Key = "EmailTemplates.TemplateKeyHelp", LanguageCode = "cs", Value = "Unikátní identifikátor šablony používaný v kódu" },
                new LocalizationString { Key = "EmailTemplates.Description", LanguageCode = "cs", Value = "Popis" },
                new LocalizationString { Key = "EmailTemplates.Placeholders", LanguageCode = "cs", Value = "Dostupné placeholdery" },
                new LocalizationString { Key = "EmailTemplates.PlaceholdersHelp", LanguageCode = "cs", Value = "Seznam placeholderů oddělených čárkou, např.: 'Name,Email,ConfirmationLink'" },
                new LocalizationString { Key = "EmailTemplates.Translations", LanguageCode = "cs", Value = "Překlady" },
                new LocalizationString { Key = "EmailTemplates.TranslationsHelp", LanguageCode = "cs", Value = "Přidejte překlady pro každý podporovaný jazyk" },
                new LocalizationString { Key = "EmailTemplates.Subject", LanguageCode = "cs", Value = "Předmět" },
                new LocalizationString { Key = "EmailTemplates.Body", LanguageCode = "cs", Value = "Obsah" },
                new LocalizationString { Key = "EmailTemplates.BodyHelp", LanguageCode = "cs", Value = "Použijte HTML pro formátování obsahu. Placeholdery vložte ve formátu {{PlaceholderName}}" },
                new LocalizationString { Key = "EmailTemplates.NoTemplates", LanguageCode = "cs", Value = "Žádné šablony nebyly nalezeny" },
                new LocalizationString { Key = "EmailTemplates.NoTranslations", LanguageCode = "cs", Value = "Chybí překlady" },
                new LocalizationString { Key = "EmailTemplates.NoPlaceholders", LanguageCode = "cs", Value = "Žádné placeholdery" },
                new LocalizationString { Key = "EmailTemplates.DeleteConfirmation", LanguageCode = "cs", Value = "Opravdu chcete smazat tuto šablonu? Tato akce je nevratná." },
                new LocalizationString { Key = "EmailTemplates.NotFound", LanguageCode = "cs", Value = "Šablona nebyla nalezena" },

                // Anglické lokalizační řetězce
                new LocalizationString { Key = "EmailTemplates.Title", LanguageCode = "en", Value = "Email Templates Management" },
                new LocalizationString { Key = "EmailTemplates.Create", LanguageCode = "en", Value = "Create Template" },
                new LocalizationString { Key = "EmailTemplates.Edit", LanguageCode = "en", Value = "Edit Template" },
                new LocalizationString { Key = "EmailTemplates.TemplateKey", LanguageCode = "en", Value = "Template Key" },
                new LocalizationString { Key = "EmailTemplates.TemplateKeyHelp", LanguageCode = "en", Value = "Unique identifier of the template used in code" },
                new LocalizationString { Key = "EmailTemplates.Description", LanguageCode = "en", Value = "Description" },
                new LocalizationString { Key = "EmailTemplates.Placeholders", LanguageCode = "en", Value = "Available Placeholders" },
                new LocalizationString { Key = "EmailTemplates.PlaceholdersHelp", LanguageCode = "en", Value = "Comma-separated list of placeholders, e.g.: 'Name,Email,ConfirmationLink'" },
                new LocalizationString { Key = "EmailTemplates.Translations", LanguageCode = "en", Value = "Translations" },
                new LocalizationString { Key = "EmailTemplates.TranslationsHelp", LanguageCode = "en", Value = "Add translations for each supported language" },
                new LocalizationString { Key = "EmailTemplates.Subject", LanguageCode = "en", Value = "Subject" },
                new LocalizationString { Key = "EmailTemplates.Body", LanguageCode = "en", Value = "Body" },
                new LocalizationString { Key = "EmailTemplates.BodyHelp", LanguageCode = "en", Value = "Use HTML for formatting the content. Insert placeholders in format {{PlaceholderName}}" },
                new LocalizationString { Key = "EmailTemplates.NoTemplates", LanguageCode = "en", Value = "No templates found" },
                new LocalizationString { Key = "EmailTemplates.NoTranslations", LanguageCode = "en", Value = "No translations" },
                new LocalizationString { Key = "EmailTemplates.NoPlaceholders", LanguageCode = "en", Value = "No placeholders" },
                new LocalizationString { Key = "EmailTemplates.DeleteConfirmation", LanguageCode = "en", Value = "Are you sure you want to delete this template? This action cannot be undone." },
                new LocalizationString { Key = "EmailTemplates.NotFound", LanguageCode = "en", Value = "Template not found" },

                // Popis emailových šablon v administraci
                new LocalizationString { Key = "Administration.EmailTemplates.Description", LanguageCode = "cs", Value = "Správa šablon emailů s podporou více jazyků a placeholderů." },
                new LocalizationString { Key = "Administration.EmailTemplates.Description", LanguageCode = "en", Value = "Manage multilingual email templates with support for placeholders." },

                // Obecné lokalizační řetězce pro tlačítka a akce
                new LocalizationString { Key = "Common.Save", LanguageCode = "cs", Value = "Uložit" },
                new LocalizationString { Key = "Common.Saving", LanguageCode = "cs", Value = "Ukládám..." },
                new LocalizationString { Key = "Common.Back", LanguageCode = "cs", Value = "Zpět" },
                new LocalizationString { Key = "Common.Edit", LanguageCode = "cs", Value = "Upravit" },
                new LocalizationString { Key = "Common.Delete", LanguageCode = "cs", Value = "Smazat" },
                new LocalizationString { Key = "Common.Actions", LanguageCode = "cs", Value = "Akce" },
                new LocalizationString { Key = "Common.Loading", LanguageCode = "cs", Value = "Načítání..." },

                new LocalizationString { Key = "Common.Save", LanguageCode = "en", Value = "Save" },
                new LocalizationString { Key = "Common.Saving", LanguageCode = "en", Value = "Saving..." },
                new LocalizationString { Key = "Common.Back", LanguageCode = "en", Value = "Back" },
                new LocalizationString { Key = "Common.Edit", LanguageCode = "en", Value = "Edit" },
                new LocalizationString { Key = "Common.Delete", LanguageCode = "en", Value = "Delete" },
                new LocalizationString { Key = "Common.Actions", LanguageCode = "en", Value = "Actions" },
                new LocalizationString { Key = "Common.Loading", LanguageCode = "en", Value = "Loading..." }
            };

            await context.LocalizationStrings.AddRangeAsync(emailTemplatesStrings);
            await context.SaveChangesAsync();
        }
    }
} 