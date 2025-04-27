using GrznarAi.Web.Client.Pages;
using GrznarAi.Web.Components;
using GrznarAi.Web.Components.Account;
using GrznarAi.Web.Data;
using GrznarAi.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Text.Json;
using GrznarAi.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<ApplicationDbContext>(opt => opt.UseSqlServer(connectionString));

// Register ProjectService
builder.Services.AddScoped<IProjectService, ProjectService>();

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

// Configure Localization
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

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
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
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using var context = await factory.CreateDbContextAsync();
    context.Database.EnsureCreated();
    context.Database.Migrate();
}

// Seed localization data
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await LocalizationDataSeeder.SeedAsync(factory);
}

// Import AI News z JSON souboru
try
{
    using (var scope = app.Services.CreateScope())
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var context = await factory.CreateDbContextAsync();
        
        // Pokud nejsou v databázi žádné AI News, importujeme je z JSON
        if (!await context.AiNewsItems.AnyAsync())
        {
            Console.WriteLine("Importuji AI News z JSON souboru...");
            
            try
            {
                // Cesta k JSON souboru
                string jsonPath = Path.Combine(app.Environment.ContentRootPath, "result.json");
                
                if (File.Exists(jsonPath))
                {
                    string jsonText = await File.ReadAllTextAsync(jsonPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var newsItems = JsonSerializer.Deserialize<List<NewsItem>>(jsonText, options);
                    
                    if (newsItems != null && newsItems.Any())
                    {
                        foreach (var item in newsItems)
                        {
                            var aiNewsItem = new AiNewsItem
                            {
                                TitleEn = item.Title ?? string.Empty,  // Původní titulek jde do obou
                                TitleCz = item.Title ?? string.Empty,  // Původní titulek jde do obou
                                ContentEn = string.Empty,              // Prázdný obsah v angličtině
                                ContentCz = item.ContentCz ?? string.Empty,
                                SummaryEn = item.SummaryEn ?? string.Empty,
                                SummaryCz = item.SummaryCz ?? string.Empty,
                                Url = item.Url ?? string.Empty,
                                ImageUrl = item.ImageUrl ?? string.Empty,
                                SourceName = item.SourceName ?? string.Empty,
                                PublishedDate = item.Date,
                                ImportedDate = item.ImportedDate,
                                IsActive = true
                            };
                            
                            context.AiNewsItems.Add(aiNewsItem);
                        }
                        
                        await context.SaveChangesAsync();
                        Console.WriteLine($"Úspěšně importováno {newsItems.Count} AI News do databáze.");
                    }
                }
                else
                {
                    Console.WriteLine($"JSON soubor nenalezen na cestě: {jsonPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při importu AI News z JSON: {ex.Message}");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Chyba při kontrole a importu AI News: {ex.Message}");
}

// Create admin accout if not exists
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
        };    

    await userManager.AddToRoleAsync(admin, roleName);
}

app.Run();
