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
builder.Services.AddScoped<IGlobalSettingsService, GlobalSettingsService>();

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

// Register GitHubService (using Octokit)
// Remove the AddHttpClient line: builder.Services.AddHttpClient<IGitHubService, GitHubService>();
builder.Services.AddScoped<IGitHubService, GitHubService>(); // Register directly

// Register LocalizationService as Singleton and Hosted Service
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
builder.Services.AddHostedService(sp => (LocalizationService)sp.GetRequiredService<ILocalizationService>());

// Register ReCaptchaService
builder.Services.AddScoped<IReCaptchaService, ReCaptchaService>();
builder.Services.AddHttpClient();

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

// Migrate database
try 
{
    using (var scope = app.Services.CreateScope())
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var context = await factory.CreateDbContextAsync();
        context.Database.EnsureCreated();
        context.Database.Migrate();
    }
}
catch (Exception ex)
{
    log.Error(ex, "Chyba při migraci databáze");
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

// Import AI News z JSON souboru

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

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            await userManager.CreateAsync(new ApplicationUser
            {
                EmailConfirmed = true,
                Email = adminEmail,
                UserName = adminEmail
            }, pass);

            admin = await userManager.FindByEmailAsync(adminEmail);
        }

        var role = await roleManager.FindByNameAsync("Admin");

        if (role is null)
        {
            await roleManager.CreateAsync(new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            });
        }
        ;

        await userManager.AddToRoleAsync(admin, roleName);
    }
}
catch (Exception ex)
{
    log.Error(ex, "Chyba při vytváření administrátorského účtu");
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