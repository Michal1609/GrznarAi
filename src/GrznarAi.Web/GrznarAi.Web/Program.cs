using GrznarAi.Web.Client.Pages;
using GrznarAi.Web.Components;
using GrznarAi.Web.Components.Account;
using GrznarAi.Web.Data;
using GrznarAi.Web.Services;
using GrznarAi.Web.Api.Middleware;
using GrznarAi.Web.Tools;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Text.Json;
using GrznarAi.Web.Models;
using Serilog;
using GrznarAi.Web.Core.Options;
using Radzen;
using GrznarAi.Web.Services.Weather;

var log = new LoggerConfiguration()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

log.Information("Starting application...");
var builder = WebApplication.CreateBuilder(args);

log.Information("AddRazorComponents...");
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Přidat kontrolery pro API
builder.Services.AddControllers();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

log.Information("connectionString...");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

log.Information("ApplicationDbContext...");
builder.Services.AddDbContextFactory<ApplicationDbContext>(opt => opt.UseSqlServer(connectionString));

// Register ProjectService
builder.Services.AddScoped<IProjectService, ProjectService>();

// Register GlobalSettingsService
builder.Services.AddSingleton<IGlobalSettingsService, GlobalSettingsService>();
builder.Services.AddHostedService(sp => (GlobalSettingsService)sp.GetRequiredService<IGlobalSettingsService>());

// Register BlogService
builder.Services.AddScoped<IBlogService, BlogService>();

// Register MarkdownService
builder.Services.AddScoped<MarkdownService>();

// Register CommentService
builder.Services.AddScoped<ICommentService, CommentService>();

// Register AiNewsService
builder.Services.AddScoped<IAiNewsService, AiNewsService>();

// Register AiNewsErrorService
builder.Services.AddScoped<IAiNewsErrorService, AiNewsErrorService>();

// Register AiNewsSourceService
builder.Services.AddScoped<IAiNewsSourceService, AiNewsSourceService>();

// Register Twitter Service and settings
builder.Services.Configure<TwitterSettings>(builder.Configuration.GetSection("TwitterSettings"));
builder.Services.AddScoped<ITwitterService, TwitterService>();

// Register Ecowitt API settings
builder.Services.Configure<EcowittApiSettings>(builder.Configuration.GetSection("EcowittApiSettings"));

// Register GitHubService (using Octokit)
// Remove the AddHttpClient line: builder.Services.AddHttpClient<IGitHubService, GitHubService>();
builder.Services.AddScoped<IGitHubService, GitHubService>(); // Register directly

// Register LocalizationService as Singleton and Hosted Service
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
builder.Services.AddHostedService(sp => (LocalizationService)sp.GetRequiredService<ILocalizationService>());

// Register ReCaptchaService
builder.Services.AddScoped<IReCaptchaService, ReCaptchaService>();
builder.Services.AddHttpClient();

// Register EmailTemplateService
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

// Register EmailService
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register PermissionService
builder.Services.AddScoped<IPermissionService, PermissionService>();

// Register NoteService
builder.Services.AddScoped<INoteService, NoteService>();

// Register CacheService as Singleton and Hosted Service
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddHostedService(sp => (CacheService)sp.GetRequiredService<ICacheService>());

// Register WeatherService
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Register Google Analytics Service
builder.Services.AddScoped<GoogleAnalyticsService>();

// Registrujeme IWeatherHistoryService
builder.Services.AddHttpClient();
builder.Services.AddScoped<IWeatherHistoryService, WeatherHistoryService>();
builder.Services.AddScoped<ITemperatureHistoryService, TemperatureHistoryService>();
builder.Services.AddScoped<IHumidityHistoryService, HumidityHistoryService>();
builder.Services.AddScoped<IPressureHistoryService, PressureHistoryService>();
builder.Services.AddScoped<IWindSpeedHistoryService, WindSpeedHistoryService>();
builder.Services.AddScoped<IWindDirectionHistoryService, WindDirectionHistoryService>();

// Registrujeme IMeteoHistoryService - upraveno pro podporu kešování
builder.Services.AddScoped<IMeteoHistoryService, MeteoHistoryService>();

// Registrujeme BackgroundTaskService jako singleton i jako HostedService
builder.Services.AddSingleton<BackgroundTaskService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BackgroundTaskService>());

// Přidání služeb Radzen
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

// Configure Localization
log.Information("Configure...");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("cs")
    };
    options.DefaultRequestCulture = new RequestCulture("en"); // Default to English
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider()); // Read from cookie first
});

log.Information("AddDatabaseDeveloperPageExceptionFilter...");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

log.Information("AddIdentityCore...");
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Registrujeme vlastní implementaci IEmailSender<ApplicationUser>
builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityEmailSender>();

log.Information("app...");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    log.Information("UseWebAssemblyDebugging...");
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    log.Information("UseExceptionHandler...");
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Lifetime.ApplicationStopping.Register(() =>
{
    log.Warning("Aplikace se vypíná.");
});

app.Lifetime.ApplicationStarted.Register(() =>
{
    log.Information("Aplikace se spustila.");
});

AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    var ex = args.ExceptionObject as Exception;
    log.Fatal(ex, "Nezachycená výjimka na úrovni AppDomain");
};

TaskScheduler.UnobservedTaskException += (sender, args) =>
{
    log.Fatal(args.Exception, "Nezachycená výjimka v tasku");
    args.SetObserved();
};

log.Information("UseHttpsRedirection...");
app.UseHttpsRedirection();

// Aktivovat middleware pro směrování
app.UseRouting();

// Přidat middleware pro autorizaci
app.UseAuthentication();
app.UseAuthorization();

// Přidat middleware pro ověření API klíčů
app.UseApiKeyMiddleware();

app.UseStaticFiles();
app.UseAntiforgery();

// Namapovat API kontrolery
app.MapControllers();

log.Information("MapRazorComponents...");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(GrznarAi.Web.Client._Imports).Assembly);

// Use Localization Middleware - IMPORTANT: Place before MapRazorComponents
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Endpoint to set language preference in a cookie
app.MapGet("/Culture/SetCulture", (string culture, string redirectUri, HttpContext context) =>
{
    if (culture != null && (culture == "cs" || culture == "en"))
    {
        context.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true } // Make cookie essential
        );
    }

    // Ensure the redirect URI is local. If not, redirect to root.
    string localRedirectUri = "/"; // Default to root
    if (!string.IsNullOrEmpty(redirectUri) && redirectUri.StartsWith("/"))
    {
        localRedirectUri = redirectUri;
    }

    return Results.LocalRedirect(localRedirectUri);
});

// Přidej na začátek programu, po registraci služeb
// Ručně inicializujeme potřebné migrace a tabulky
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        bool migrationsTableExists = false;
        
        try
        {
            // Kontrola existence tabulky __EFMigrationsHistory
            migrationsTableExists = db.Database.ExecuteSqlRaw(
                "SELECT COUNT(*) FROM __EFMigrationsHistory") > 0;
        }
        catch
        {
            // Tabulka neexistuje
        }
        
        if (migrationsTableExists)
        {
            // Kontrola, jestli migrace existuje v tabulce
            var fixCyclicCascadeMigrationExists = db.Database.ExecuteSqlRaw(
                "SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId = '20250501121547_FixCyclicCascade'") > 0;
                
            if (!fixCyclicCascadeMigrationExists)
            {
                // Vložení záznamu o migraci do tabulky __EFMigrationsHistory
                db.Database.ExecuteSqlRaw(
                    "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20250501121547_FixCyclicCascade', '8.0.0')");
                
                Console.WriteLine("Migrace FixCyclicCascade byla ručně zaznamenána do __EFMigrationsHistory.");
            }
            
            // Kontrola, jestli existují tabulky pro EmailTemplate
            bool emailTemplatesTableExists = false;
            
            try
            {
                emailTemplatesTableExists = db.Database.ExecuteSqlRaw(
                    "SELECT COUNT(*) FROM EmailTemplates") >= 0;
            }
            catch
            {
                // Tabulka neexistuje
            }
            
            if (!emailTemplatesTableExists)
            {
                // Vytvoření tabulky EmailTemplates
                db.Database.ExecuteSqlRaw(@"
                    CREATE TABLE [EmailTemplates] (
                        [Id] int NOT NULL IDENTITY,
                        [TemplateKey] nvarchar(100) NOT NULL,
                        [Description] nvarchar(255) NOT NULL,
                        [AvailablePlaceholders] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_EmailTemplates] PRIMARY KEY ([Id])
                    );
                    
                    CREATE UNIQUE INDEX [IX_EmailTemplates_TemplateKey] 
                    ON [EmailTemplates] ([TemplateKey]);
                ");
                
                Console.WriteLine("Tabulka EmailTemplates byla vytvořena ručně.");
            }
            
            // Kontrola, jestli existují tabulky pro EmailTemplateTranslation
            bool emailTemplateTranslationsTableExists = false;
            
            try
            {
                emailTemplateTranslationsTableExists = db.Database.ExecuteSqlRaw(
                    "SELECT COUNT(*) FROM EmailTemplateTranslations") >= 0;
            }
            catch
            {
                // Tabulka neexistuje
            }
            
            if (!emailTemplateTranslationsTableExists)
            {
                // Vytvoření tabulky EmailTemplateTranslations
                db.Database.ExecuteSqlRaw(@"
                    CREATE TABLE [EmailTemplateTranslations] (
                        [Id] int NOT NULL IDENTITY,
                        [EmailTemplateId] int NOT NULL,
                        [LanguageCode] nvarchar(10) NOT NULL,
                        [Subject] nvarchar(255) NOT NULL,
                        [Body] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_EmailTemplateTranslations] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_EmailTemplateTranslations_EmailTemplates_EmailTemplateId] 
                            FOREIGN KEY ([EmailTemplateId]) REFERENCES [EmailTemplates] ([Id]) ON DELETE CASCADE
                    );
                    
                    CREATE UNIQUE INDEX [IX_EmailTemplateTranslations_EmailTemplateId_LanguageCode] 
                    ON [EmailTemplateTranslations] ([EmailTemplateId], [LanguageCode]);
                ");
                
                Console.WriteLine("Tabulka EmailTemplateTranslations byla vytvořena ručně.");
            }
            
            // Vložení záznamu o migraci AddEmailTemplates do tabulky __EFMigrationsHistory
            var addEmailTemplatesMigrationExists = db.Database.ExecuteSqlRaw(
                "SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId = '20250502103228_AddEmailTemplates'") > 0;
                
            if (!addEmailTemplatesMigrationExists)
            {
                // Vložení záznamu o migraci do tabulky __EFMigrationsHistory
                db.Database.ExecuteSqlRaw(
                    "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20250502103228_AddEmailTemplates', '8.0.0')");
                
                Console.WriteLine("Migrace AddEmailTemplates byla ručně zaznamenána do __EFMigrationsHistory.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Chyba při ručním zpracování migrace: {ex.Message}");
    }
}

// Migrate database
try 
{
    using (var scope = app.Services.CreateScope())
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var context = await factory.CreateDbContextAsync();
        
        // Použijeme pouze metodu EnsureCreated, která vytvoří databázi, pokud neexistuje
        // A nezpůsobuje konflikt s migrací jako kombinace EnsureCreated + Migrate
        var dbExists = await context.Database.EnsureCreatedAsync();
        if (dbExists)
        {
            log.Information("Databáze již existuje, EnsureCreated neprovede žádné změny");            
        }
        else 
        {
            log.Information("Databáze byla vytvořena metodou EnsureCreated");
        }
        context.Database.Migrate();
    }
}
catch (Exception ex)
{
    log.Error(ex, "Chyba při inicializaci databáze");
}

// Seed localization data
try
{
    using (var scope = app.Services.CreateScope())
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        await LocalizationDataSeeder.SeedAsync(factory);
    }
}
catch (Exception ex)
{
    log.Error(ex, "Chyba při seedování lokalizačních dat");
}

// Seed application permissions
try
{
    using (var scope = app.Services.CreateScope())
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        await ApplicationPermissionSeeder.SeedAsync(factory);
    }
}
catch (Exception ex)
{
    log.Error(ex, "Chyba při seedování oprávnění aplikace");
}

// Seed email templates
try
{
    using (var scope = app.Services.CreateScope())
    {
        log.Information("Seedování emailových šablon...");
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailTemplateDataSeeder>>();
        var seeder = new EmailTemplateDataSeeder(logger, dbContext);
        await seeder.SeedAsync();
    }
}
catch (Exception ex)
{
    log.Error(ex, "Chyba při seedování emailových šablon");
}

// Create admin accout if not exists
try
{
    using (var scope = app.Services.CreateScope())
    {
        var adminEmail = "admin@admin.com";
        var pass = "Admin123!";
        var roleName = "Admin";
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Nejprve zkontrolujeme, zda existuje role Admin
        var adminRole = await roleManager.FindByNameAsync(roleName);
        if (adminRole is null)
        {
            log.Information("Vytvářím roli Admin, protože neexistuje");
            await roleManager.CreateAsync(new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            });
            
            adminRole = await roleManager.FindByNameAsync(roleName);
        }

        // Zjistíme, zda existuje nějaký uživatel s rolí Admin
        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
        var usersInRole = await userManager.GetUsersInRoleAsync(roleName);
        
        if (usersInRole == null || !usersInRole.Any())
        {
            log.Information("Neexistuje žádný uživatel s rolí Admin. Vytvářím výchozího administrátora.");
            
            // Vytvoření nového administrátora
            var admin = new ApplicationUser
            {
                EmailConfirmed = true,
                Email = adminEmail,
                UserName = adminEmail
            };
            
            var result = await userManager.CreateAsync(admin, pass);
            
            if (result.Succeeded)
            {
                log.Information("Admin účet byl úspěšně vytvořen.");
                await userManager.AddToRoleAsync(admin, roleName);
                log.Information("Admin role byla přiřazena uživateli.");
            }
            else
            {
                log.Error("Nepodařilo se vytvořit admin účet: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            log.Information("Administrátoři již existují v systému. Přeskakuji vytváření výchozího admina.");
        }
    }
}
catch (Exception ex)
{
    log.Error(ex, "Chyba při kontrole/vytváření administrátorského účtu");
}

// Vytvoření výchozího API klíče, pokud není žádný v databázi
try
{
    using var scope = app.Services.CreateScope();
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using var context = await contextFactory.CreateDbContextAsync();
    
    // Kontrola, zda již existuje nějaký API klíč
    if (!await context.ApiKeys.AnyAsync())
    {
        Console.WriteLine("Vytvářím výchozí API klíč...");
        await ApiKeyGenerator.GenerateApiKey(app, "Default API Key", "Výchozí API klíč pro přístup k API", 365);
    }
}
catch (Exception ex)
{
    log.Error(ex, "Chyba při vytváření výchozího API klíče");
    Console.WriteLine($"Chyba při vytváření výchozího API klíče: {ex.Message}");
}

app.Run(); 