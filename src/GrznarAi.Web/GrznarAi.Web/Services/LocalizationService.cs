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
                    var cultureCode = GetBaseCulture(locString.LanguageCode);
                    if (!newCache.TryGetValue(cultureCode, out var cultureDict))
                    {
                        cultureDict = new ConcurrentDictionary<string, string>();
                        newCache.TryAdd(cultureCode, cultureDict);
                    }
                    // Add or update the value for the key in the specific culture dictionary
                    cultureDict[locString.Key] = locString.Value; 
                }
                
                // Atomically replace the cache
                Interlocked.Exchange(ref _cache, newCache);

                // Log counts per culture
                var cultureCounts = newCache.Keys.Select(c => $"{c.ToUpper()}: {newCache[c].Count}");
                _logger.LogInformation("Localization cache reloaded successfully. Key counts: {Counts}", string.Join(", ", cultureCounts));
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
            var cultureCode = GetBaseCulture(culture);

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
                _logger.LogWarning("Invalid culture code '{Culture}' provided. Defaulting to 'en'.", culture);
                return "en"; // Default to English if culture is invalid or not found
            }
        }

        // --- Admin Methods --- 

        public async Task<List<LocalizationString>> GetAllStringsAdminAsync()
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Order by Key then LanguageCode for better grouping in potential UI
            return await dbContext.LocalizationStrings.OrderBy(s => s.Key).ThenBy(s => s.LanguageCode).ToListAsync();
        }

        public async Task<LocalizationString?> GetSingleStringAdminAsync(int id)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await dbContext.LocalizationStrings.FindAsync(id);
        }

        public async Task AddStringAsync(LocalizationString localizationString)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(localizationString.Key) || string.IsNullOrWhiteSpace(localizationString.LanguageCode))
            {
                throw new ArgumentException("Key and LanguageCode cannot be empty.");
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Check for duplicates explicitly before adding (due to composite key)
            bool exists = await dbContext.LocalizationStrings.AnyAsync(ls => ls.Key == localizationString.Key && ls.LanguageCode == localizationString.LanguageCode);
            if(exists)
            {
                 throw new InvalidOperationException($"Localization string with Key '{localizationString.Key}' and LanguageCode '{localizationString.LanguageCode}' already exists.");
            }

            dbContext.LocalizationStrings.Add(localizationString);
            await dbContext.SaveChangesAsync();
            await ReloadCacheAsync();
        }

        public async Task UpdateStringAsync(LocalizationString localizationString)
        {
             if (localizationString.Id <= 0)
            {
                throw new ArgumentException("Cannot update a localization string without a valid Id.");
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure the Key/LanguageCode combination isn't changing to conflict with another existing entry
            // (This check might be complex depending on desired behavior - basic check here)
            bool conflictingEntryExists = await dbContext.LocalizationStrings
                .AnyAsync(ls => ls.Id != localizationString.Id && ls.Key == localizationString.Key && ls.LanguageCode == localizationString.LanguageCode);

            if (conflictingEntryExists)
            {
                throw new InvalidOperationException($"Another localization string with Key '{localizationString.Key}' and LanguageCode '{localizationString.LanguageCode}' already exists.");
            }


            // Attach and mark as modified (or fetch then update)
            dbContext.Entry(localizationString).State = EntityState.Modified; 
            // Alternatively: 
            // var existing = await dbContext.LocalizationStrings.FindAsync(localizationString.Id);
            // if (existing != null) { /* update properties */ }

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                 _logger.LogError(ex, "Concurrency error updating localization string with Id {Id}", localizationString.Id);
                // Handle concurrency conflict (e.g., re-fetch and notify user)
                throw; // Re-throw for now
            }
            await ReloadCacheAsync();
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
                await ReloadCacheAsync();
            }
            else
            {
                 _logger.LogWarning("Attempted to delete non-existent localization string with Id {Id}", id);
            }
        }

        public async Task<bool> InstallFromJsonAsync(string? jsonPath = null)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
                await LocalizationDataSeeder.SeedAsync(factory, jsonPath);
                await ReloadCacheAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při instalaci lokalizací ze seedovacího JSON souboru.");
                return false;
            }
        }

        public async Task DeleteAllStringsAsync()
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var all = await dbContext.LocalizationStrings.ToListAsync();
            dbContext.LocalizationStrings.RemoveRange(all);
            await dbContext.SaveChangesAsync();
            await ReloadCacheAsync();
        }
    }
} 