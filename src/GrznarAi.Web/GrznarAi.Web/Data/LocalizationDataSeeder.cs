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

            // Historical data
            AddEntry("Meteo.HistoricalData", "Historická data", "Historical Data", "Historical data section title");
            AddEntry("Meteo.HistoricalData.SameDay", "Tento den v historii", "This Day in History", "Same day statistics title");
            AddEntry("Meteo.HistoricalData.YearlyStats", "Roční statistiky", "Yearly Statistics", "Yearly statistics title");
            AddEntry("Meteo.HistoricalData.NoData", "Žádná data nejsou k dispozici", "No data available", "No data available message");
            AddEntry("Meteo.HistoricalData.Year", "Rok", "Year", "Year column");
            AddEntry("Meteo.HistoricalData.MinTemp", "Min. teplota", "Min. Temp", "Minimum temperature column");
            AddEntry("Meteo.HistoricalData.AvgTemp", "Průměr. teplota", "Avg. Temp", "Average temperature column");
            AddEntry("Meteo.HistoricalData.MaxTemp", "Max. teplota", "Max. Temp", "Maximum temperature column");
            AddEntry("Meteo.HistoricalData.Rainfall", "Srážky", "Rainfall", "Rainfall column");
            AddEntry("Meteo.HistoricalData.AvgHumidity", "Průměr. vlhkost", "Avg. Humidity", "Average humidity column");
            AddEntry("Meteo.HistoricalData.LastFrostDay", "Poslední mráz", "Last Frost", "Last frost day in first half of year column");
            AddEntry("Meteo.HistoricalData.FirstFrostDay", "První mráz", "First Frost", "First frost day in second half of year column");
            AddEntry("Meteo.HistoricalData.FrostDays", "Mrazivé dny", "Frost Days", "Number of frost days column");
            AddEntry("Meteo.HistoricalData.FirstHotDay", "První tropický den", "First Tropical Day", "First hot day column");
            AddEntry("Meteo.HistoricalData.LastHotDay", "Poslední tropický den", "Last Tropical Day", "Last hot day column");
            AddEntry("Meteo.HistoricalData.HotDays", "Tropické dny", "Tropical Days", "Number of hot days column");
            AddEntry("Meteo.HistoricalData.MinYearTemp", "Min. teplota", "Min. Temp", "Minimum yearly temperature column");
            AddEntry("Meteo.HistoricalData.MaxYearTemp", "Max. teplota", "Max. Temp", "Maximum yearly temperature column");
            AddEntry("Meteo.HistoricalData.AvgYearTemp", "Průměr. teplota", "Avg. Temp", "Average yearly temperature column");
            AddEntry("Meteo.HistoricalData.YearlyRainfall", "Roční srážky", "Yearly Rainfall", "Yearly rainfall column");

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

            // Lokalizační řetězce pro emailové šablony
            AddEntry("EmailTemplates.Title", "Správa emailových šablon", "Email Templates Management", "Email templates management title");
            AddEntry("EmailTemplates.Create", "Vytvořit šablonu", "Create Template", "Create template button");
            AddEntry("EmailTemplates.Edit", "Upravit šablonu", "Edit Template", "Edit template button");
            AddEntry("EmailTemplates.TemplateKey", "Klíč šablony", "Template Key", "Template key label");
            AddEntry("EmailTemplates.TemplateKeyHelp", "Unikátní identifikátor šablony používaný v kódu", "Unique identifier of the template used in code", "Template key help text");
            AddEntry("EmailTemplates.Description", "Popis", "Description", "Description label");
            AddEntry("EmailTemplates.Placeholders", "Dostupné placeholdery", "Available Placeholders", "Placeholders label");
            AddEntry("EmailTemplates.PlaceholdersHelp", "Seznam placeholderů oddělených čárkou, např.: 'Name,Email,ConfirmationLink'", "Comma-separated list of placeholders, e.g.: 'Name,Email,ConfirmationLink'", "Placeholders help text");
            AddEntry("EmailTemplates.Translations", "Překlady", "Translations", "Translations label");
            AddEntry("EmailTemplates.TranslationsHelp", "Přidejte překlady pro každý podporovaný jazyk", "Add translations for each supported language", "Translations help text");
            AddEntry("EmailTemplates.Subject", "Předmět", "Subject", "Subject label");
            AddEntry("EmailTemplates.Body", "Obsah", "Body", "Body label");
            AddEntry("EmailTemplates.BodyHelp", "Použijte HTML pro formátování obsahu. Placeholdery vložte ve formátu {{PlaceholderName}}", "Use HTML for formatting the content. Insert placeholders in format {{PlaceholderName}}", "Body help text");
            AddEntry("EmailTemplates.NoTemplates", "Žádné šablony nebyly nalezeny", "No templates found", "No templates message");
            AddEntry("EmailTemplates.NoTranslations", "Chybí překlady", "No translations", "No translations message");
            AddEntry("EmailTemplates.NoPlaceholders", "Žádné placeholdery", "No placeholders", "No placeholders message");
            AddEntry("EmailTemplates.DeleteConfirmation", "Opravdu chcete smazat tuto šablonu? Tato akce je nevratná.", "Are you sure you want to delete this template? This action cannot be undone.", "Delete confirmation message");
            AddEntry("EmailTemplates.NotFound", "Šablona nebyla nalezena", "Template not found", "Template not found message");

            // Popis emailových šablon v administraci
            AddEntry("Administration.EmailTemplates.Description", "Správa šablon emailů s podporou více jazyků a placeholderů.", "Manage multilingual email templates with support for placeholders.", "Email templates administration description");

            // Obecné lokalizační řetězce pro tlačítka a akce
            AddEntry("Common.Save", "Uložit", "Save", "Save button");
            AddEntry("Common.Saving", "Ukládám...", "Saving...", "Saving message");
            AddEntry("Common.Back", "Zpět", "Back", "Back button");
            AddEntry("Common.Edit", "Upravit", "Edit", "Edit button");
            AddEntry("Common.Delete", "Smazat", "Delete", "Delete button");
            AddEntry("Common.Actions", "Akce", "Actions", "Actions column header");
            AddEntry("Common.Loading", "Načítání...", "Loading...", "Loading message");

            // --- Administration Users Seed Data ---
            AddEntry("Administration.Users.Title", "Administrace uživatelů", "User Administration", "User administration page title");
            AddEntry("Administration.Users.List", "Seznam uživatelů", "User List", "User list section title");
            AddEntry("Administration.Users.Create", "Vytvořit uživatele", "Create User", "Create user button");
            AddEntry("Administration.Users.Filter", "Filtrovat uživatele...", "Filter users...", "Filter users placeholder");
            AddEntry("Administration.Filter", "Filtrovat", "Filter", "Filter button text");
            AddEntry("Administration.Reset", "Reset", "Reset", "Reset filter button text");
            AddEntry("Administration.Users.ManageRoles", "Spravovat role", "Manage Roles", "Manage roles button");
            AddEntry("Administration.Loading", "Načítání...", "Loading...", "Loading message");
            AddEntry("Administration.Users.NoUsersFound", "Nebyli nalezeni žádní uživatelé", "No users found", "No users found message");
            AddEntry("Administration.Users.Username", "Uživatelské jméno", "Username", "Username column header");
            AddEntry("Administration.Users.Email", "Email", "Email", "Email column header");
            AddEntry("Administration.Users.EmailConfirmed", "Email potvrzen", "Email Confirmed", "Email confirmed column header");
            AddEntry("Administration.Users.Roles", "Role", "Roles", "Roles column header");
            AddEntry("Administration.Actions", "Akce", "Actions", "Actions column header");
            AddEntry("Administration.Confirmation", "Potvrzení", "Confirmation", "Confirmation dialog title");
            AddEntry("Administration.Users.ConfirmDelete", "Opravdu chcete smazat tohoto uživatele?", "Are you sure you want to delete this user?", "Delete user confirmation message");
            AddEntry("Administration.No", "Ne", "No", "No button text");
            AddEntry("Administration.Yes", "Ano", "Yes", "Yes button text");
            AddEntry("Administration.Back", "Zpět", "Back", "Back button text");
            AddEntry("Administration.Users.Edit", "Upravit uživatele", "Edit User", "Edit user page title");
            AddEntry("Administration.Users.LockoutEnabled", "Povoleno uzamčení", "Lockout Enabled", "Lockout enabled checkbox label");
            AddEntry("Administration.Permissions.Title", "Oprávnění", "Permissions", "Permissions section title");
            AddEntry("Administration.Permissions.UserPermissions", "Uživatelská oprávnění", "User Permissions", "User permissions section title");
            AddEntry("Administration.Permissions.NoPermissions", "Žádná oprávnění", "No permissions", "No permissions message");
            AddEntry("Administration.Permissions.SelectPermission", "Dostupná oprávnění", "Available Permissions", "Available permissions section title");
            AddEntry("Administration.Saving", "Ukládání...", "Saving...", "Saving message");
            AddEntry("Administration.Save", "Uložit", "Save", "Save button text");

            // --- Administration Users Password Management ---
            AddEntry("Administration.Users.Password.New", "Nové heslo", "New Password", "New password label");
            AddEntry("Administration.Users.Password.Confirm", "Potvrzení hesla", "Confirm Password", "Confirm password label");
            AddEntry("Administration.Users.Password.Change", "Změnit heslo", "Change Password", "Change password button");
            AddEntry("Administration.Users.Password.LengthError", "Heslo musí být dlouhé alespoň {0} znaků.", "Password must be at least {0} characters long.", "Password length validation message");
            AddEntry("Administration.Users.Password.ComplexityError", "Heslo musí obsahovat velké písmeno, malé písmeno, číslici a speciální znak.", "Password must contain uppercase letter, lowercase letter, digit, and special character.", "Password complexity validation message");
            AddEntry("Administration.Users.Password.MatchError", "Hesla se neshodují", "Passwords do not match", "Password match validation message");
            AddEntry("Administration.Users.UsernameRequired", "Uživatelské jméno je povinné", "Username is required", "Username required validation message");
            AddEntry("Administration.Users.EmailRequired", "Email je povinný", "Email is required", "Email required validation message");
            AddEntry("Administration.Users.EmailInvalid", "Neplatný formát emailu", "Invalid email format", "Email format validation message");
            AddEntry("Administration.Users.NotFound", "Uživatel nebyl nalezen.", "User not found.", "User not found message");
            AddEntry("Administration.Users.UpdateSuccess", "Uživatel byl úspěšně aktualizován.", "User was successfully updated.", "User update success message");
            AddEntry("Administration.Users.LoadError", "Chyba při načítání uživatele: {0}", "Error loading user: {0}", "User loading error message");
            AddEntry("Administration.Users.SaveError", "Chyba při ukládání uživatele: {0}", "Error saving user: {0}", "User saving error message");
            AddEntry("Administration.Users.PermissionsLoadError", "Chyba při načítání oprávnění: {0}", "Error loading permissions: {0}", "Permissions loading error message");
            AddEntry("Administration.Users.CannotDeleteSelf", "Nemůžete smazat svůj vlastní účet.", "You cannot delete your own account.", "Error message when admin tries to delete themselves");

            // --- End of new sections ---

            // --- Administration WeatherHistory --- 
            AddEntry("Administration.WeatherHistory.Title", "Správa historie počasí", "Weather History Management", "Weather history admin page title");
            AddEntry("Administration.WeatherHistory.Description", "Importujte historická data z CSV souborů nebo stáhněte aktuální data z Ecowitt API.", "Import historical data from CSV files or fetch current data from Ecowitt API.", "Weather history admin page description");
            AddEntry("Administration.WeatherHistory.ImportCsv", "Import CSV souborů", "Import CSV Files", "CSV import section title");
            AddEntry("Administration.WeatherHistory.ImportDescription", "Nahrání CSV souborů s historickými daty meteostanice. Soubory musí mít správný formát s hlavičkou a oddělovačem středníkem.", "Upload CSV files containing weather station historical data. Files must have correct format with header and semicolon separator.", "CSV import section description");
            AddEntry("Administration.WeatherHistory.UploadFiles", "Nahrát soubory", "Upload Files", "Upload button text");
            AddEntry("Administration.WeatherHistory.SelectedFiles", "Vybrané soubory", "Selected Files", "Selected files label");
            AddEntry("Administration.WeatherHistory.UploadSuccess", "Úspěšně nahráno", "Successfully uploaded", "Upload success message");
            AddEntry("Administration.WeatherHistory.UploadError", "Chyba při nahrávání", "Error uploading", "Upload error message");
            AddEntry("Administration.WeatherHistory.ProcessedSuccessfully", "Úspěšně zpracováno", "Processed successfully", "File processed successfully message");
            AddEntry("Administration.WeatherHistory.ProcessingError", "Chyba při zpracování", "Processing error", "File processing error message");
            AddEntry("Administration.WeatherHistory.FileTooLarge", "Soubor je příliš velký", "File is too large", "File too large message");
            AddEntry("Administration.WeatherHistory.FetchData", "Stažení dat z Ecowitt API", "Fetch Data from Ecowitt API", "Fetch data section title");
            AddEntry("Administration.WeatherHistory.FetchDescription", "Stáhněte aktuální data z Ecowitt API. Data budou stažena od posledního záznamu v databázi.", "Fetch current data from Ecowitt API. Data will be fetched from the last record in database.", "Fetch data section description");
            AddEntry("Administration.WeatherHistory.FetchButton", "Stáhnout data", "Fetch Data", "Fetch data button text");
            AddEntry("Administration.WeatherHistory.FetchSuccess", "Data byla úspěšně stažena", "Data was successfully fetched", "Successful fetch message");
            AddEntry("Administration.WeatherHistory.FetchError", "Chyba při stahování dat", "Error fetching data", "Fetch error message");
            AddEntry("Administration.WeatherHistory.Stats", "Statistika", "Statistics", "Statistics section title");
            AddEntry("Administration.WeatherHistory.LastRecord", "Poslední záznam", "Last record", "Last weather record date label");
            AddEntry("Administration.WeatherHistory.TotalRecords", "Celkem záznamů", "Total records", "Total weather records count label");
            AddEntry("Administration.WeatherHistory.NoRecords", "Žádné záznamy v databázi", "No records in database", "No weather records found message");
            AddEntry("Common.Uploading", "Nahrávám", "Uploading", "Uploading text");
            AddEntry("Administration.WeatherHistory.FileLimit", "Limit: {0} souborů najednou", "Limit: {0} files at once", "File upload limit text");

            // --- Admin menu item ---
            AddEntry("NavMenu.Admin.WeatherHistory", "Historie počasí", "Weather History", "Admin menu item - Weather History");

            // --- Automatic fetch settings ---
            AddEntry("Administration.WeatherHistory.AutoFetch", "Automatické stahování dat", "Automatic data fetching", "Automatic data fetch section title");
            AddEntry("Administration.WeatherHistory.AutoFetchDescription", "Zapněte nebo vypněte automatické stahování dat z Ecowitt API každých 10 minut.", "Enable or disable automatic data fetching from Ecowitt API every 10 minutes.", "Automatic data fetch description");
            AddEntry("Administration.WeatherHistory.AutoFetchEnabled", "Automatické stahování dat je ZAPNUTO", "Automatic data fetching is ENABLED", "Automatic data fetch enabled label");
            AddEntry("Administration.WeatherHistory.AutoFetchDisabled", "Automatické stahování dat je VYPNUTO", "Automatic data fetching is DISABLED", "Automatic data fetch disabled label");
            AddEntry("Administration.WeatherHistory.AutoFetchRunning", "Služba bude automaticky stahovat data každých 10 minut.", "Service will automatically fetch data every 10 minutes.", "Automatic data fetch running info");
            AddEntry("Administration.WeatherHistory.AutoFetchStopped", "Služba pro automatické stahování je zastavena. Můžete stále používat ruční stahování.", "Automatic data fetching service is stopped. You can still use manual fetching.", "Automatic data fetch stopped info");
            
            // --- Process files ---
            AddEntry("Common.Errors", "Chyby", "Errors", "Errors section title");
            AddEntry("Common.Processing", "Zpracovávám", "Processing", "Processing action label");
            AddEntry("Administration.WeatherHistory.ProcessFiles", "Zpracovat soubory", "Process files", "Process files button text");
            
            // --- Fetch for period ---
            AddEntry("Administration.WeatherHistory.FetchPeriodTitle", "Stažení dat za specifické období", "Fetch data for specific period", "Fetch data for period section title");
            AddEntry("Administration.WeatherHistory.FetchPeriodDescription", "Stáhněte data od zadaného data až po současnost. Maximální interval je 24 hodin.", "Fetch data from specified date to present. Maximum interval is 24 hours.", "Fetch data for period description");
            AddEntry("Administration.WeatherHistory.StartDate", "Počáteční datum", "Start date", "Start date label");
            AddEntry("Administration.WeatherHistory.FetchPeriodButton", "Stáhnout data za období", "Fetch data for period", "Fetch data for period button text");
            AddEntry("Administration.WeatherHistory.FetchPeriodSuccess", "Data za zvolené období byla úspěšně stažena", "Data for selected period was successfully fetched", "Fetch data for period success message");
            AddEntry("Administration.WeatherHistory.FetchPeriodError", "Chyba při stahování dat za zvolené období", "Error fetching data for selected period", "Fetch data for period error message");

            // --- Error and confirmation messages ---
            AddEntry("Administration.WeatherHistory.ErrorUpdatingSettings", "Chyba při aktualizaci nastavení", "Error updating settings", "Error message when settings update fails");
            AddEntry("Administration.WeatherHistory.ConfirmDeleteHistory", "Opravdu chcete smazat všechna historická data? Tato akce je nevratná.", "Are you sure you want to delete all historical data? This action cannot be undone.", "Confirmation message for deleting all history");
            AddEntry("Administration.WeatherHistory.DataDeletedSuccess", "Všechna historická data byla úspěšně smazána", "All historical data was successfully deleted", "Success message after deleting all history");
            AddEntry("Administration.WeatherHistory.DataDeletedError", "Chyba při mazání historických dat", "Error deleting historical data", "Error message when deleting history fails");
            AddEntry("Administration.WeatherHistory.ErrorLoadingStats", "Chyba při načítání statistik historie počasí", "Error loading weather history statistics", "Error message when loading statistics fails");

            // --- Meteo Trends Page ---
            AddEntry("Meteo.Trends", "Vývoj", "Trends", "Button for weather trends page");
            AddEntry("Meteo.Trends.Title", "Vývoj počasí", "Weather Trends", "Weather trends page title");
            AddEntry("Meteo.Trends.SelectPeriod", "Vyberte období", "Select Period", "Period selector title");
            AddEntry("Meteo.Trends.Day", "Den", "Day", "Day option for period selection");
            AddEntry("Meteo.Trends.Week", "Týden", "Week", "Week option for period selection");
            AddEntry("Meteo.Trends.Month", "Měsíc", "Month", "Month option for period selection");
            AddEntry("Meteo.Trends.Year", "Rok", "Year", "Year option for period selection");
            AddEntry("Meteo.Trends.Temperature", "Teplota", "Temperature", "Temperature chart title");
            AddEntry("Meteo.Trends.MinTemperature", "Min. teplota", "Min. Temperature", "Min temperature label");
            AddEntry("Meteo.Trends.AvgTemperature", "Prům. teplota", "Avg. Temperature", "Avg temperature label");
            AddEntry("Meteo.Trends.MaxTemperature", "Max. teplota", "Max. Temperature", "Max temperature label");
            AddEntry("Meteo.Trends.Humidity", "Vlhkost", "Humidity", "Humidity chart title");
            AddEntry("Meteo.Trends.Pressure", "Atmosférický tlak", "Atmospheric Pressure", "Pressure chart title");
            AddEntry("Meteo.Trends.WindSpeed", "Rychlost větru", "Wind Speed", "Wind speed chart title");
            AddEntry("Meteo.Trends.Rainfall", "Srážky", "Rainfall", "Rainfall chart title");
            AddEntry("Meteo.Trends.SolarRadiation", "Sluneční záření", "Solar Radiation", "Solar radiation chart title");
            AddEntry("Meteo.Trends.UVIndex", "UV index", "UV Index", "UV index chart title");
            AddEntry("Meteo.Trends.DateTime", "Datum a čas", "Date & Time", "DateTime axis label");
            AddEntry("Meteo.Trends.NoData", "Pro vybrané období nejsou k dispozici žádná data", "No data available for the selected period", "No data message");
            
            // Přidané lokalizační řetězce pro časové intervaly grafů
            AddEntry("Meteo.Trends.Hours", "Hodiny", "Hours", "Hours axis label");
            AddEntry("Meteo.Trends.FourHourInterval", "Čtyřhodinové intervaly", "4-hour Intervals", "4-hour intervals axis label");
            AddEntry("Meteo.Trends.Days", "Dny", "Days", "Days axis label");
            AddEntry("Meteo.Trends.Weeks", "Týdny", "Weeks", "Weeks axis label");
            
            // --- Historical Data Section ---

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