using System;
using System.Collections.Concurrent; // For thread-safe dictionary
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // For IServiceScopeFactory
using Microsoft.Extensions.Hosting; // For IHostedService
using Microsoft.Extensions.Logging;

namespace GrznarAi.Web.Services
{
    // Service implementation
    public class LocalizationService : ILocalizationService, IHostedService // Implement IHostedService to load cache on startup
    {
        private readonly ILogger<LocalizationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Use scope factory to create DbContext scopes
        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _cache = new(); // Cache: Culture -> Key -> Value

        public LocalizationService(ILogger<LocalizationService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        // Load cache on application startup
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("LocalizationService starting. Initializing cache...");
            return InitializeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("LocalizationService stopping.");
            _cache.Clear();
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await ReloadCacheAsync();
        }
        
        public async Task ReloadCacheAsync()
        {
             _logger.LogInformation("Reloading localization cache...");
             var newCache = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

             try
             {
                await using var scope = _scopeFactory.CreateAsyncScope();
                await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var allStrings = await dbContext.LocalizationStrings.AsNoTracking().ToListAsync();

                foreach (var locString in allStrings)
                {
                    // Add CS value
                    if (!newCache.TryGetValue("cs", out var csDict))
                    {
                        csDict = new ConcurrentDictionary<string, string>();
                        newCache.TryAdd("cs", csDict);
                    }
                    csDict.TryAdd(locString.Key, locString.ValueCs);

                    // Add EN value
                    if (!newCache.TryGetValue("en", out var enDict))
                    {
                        enDict = new ConcurrentDictionary<string, string>();
                        newCache.TryAdd("en", enDict);
                    }
                    enDict.TryAdd(locString.Key, locString.ValueEn);
                }
                
                // Atomically replace the cache
                Interlocked.Exchange(ref _cache, newCache);
                _logger.LogInformation("Localization cache reloaded successfully with strings for {CountCs} CS keys and {CountEn} EN keys.", 
                    _cache.TryGetValue("cs", out var cs) ? cs.Count : 0,
                    _cache.TryGetValue("en", out var en) ? en.Count : 0);
             }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading localization cache from database.");
            }
        }

        // Get string based on current UI culture
        public string GetString(string key)
        {
            return GetString(key, CultureInfo.CurrentUICulture.Name);
        }

        // Get string based on specific culture
        public string GetString(string key, string culture)
        {
            var cultureCode = GetBaseCulture(culture); // e.g., "cs-CZ" -> "cs"

            if (_cache.TryGetValue(cultureCode, out var cultureDict) && cultureDict.TryGetValue(key, out var value))
            {
                return value;
            }

            _logger.LogWarning("Localization key '{Key}' not found for culture '{Culture}'.", key, cultureCode);
            // Fallback: Return the key itself or a default marker
            return $"[{key}]";
        }

        // Helper to get base culture (e.g., "cs" from "cs-CZ")
        private string GetBaseCulture(string culture)
        {
            try
            {
                return new CultureInfo(culture).TwoLetterISOLanguageName;
            }
            catch (CultureNotFoundException)
            {
                return "en"; // Default to English if culture is invalid
            }
        }

        // --- Admin Methods --- 

        public async Task<List<LocalizationString>> GetAllStringsAdminAsync()
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await dbContext.LocalizationStrings.OrderBy(s => s.Key).ToListAsync();
        }

        public async Task<LocalizationString?> GetStringByIdAsync(int id)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await dbContext.LocalizationStrings.FindAsync(id);
        }

        public async Task AddStringAsync(LocalizationString localizationString)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.LocalizationStrings.Add(localizationString);
            await dbContext.SaveChangesAsync();
            await ReloadCacheAsync(); // Reload cache after adding
        }

        public async Task UpdateStringAsync(LocalizationString localizationString)
        {
             await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Entry(localizationString).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            await ReloadCacheAsync(); // Reload cache after updating
        }

        public async Task DeleteStringAsync(int id)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var item = await dbContext.LocalizationStrings.FindAsync(id);
            if (item != null)
            {
                dbContext.LocalizationStrings.Remove(item);
                await dbContext.SaveChangesAsync();
                await ReloadCacheAsync(); // Reload cache after deleting
            }
        }
    }
} 