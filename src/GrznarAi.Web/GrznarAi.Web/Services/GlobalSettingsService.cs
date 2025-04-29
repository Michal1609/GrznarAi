using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrznarAi.Web.Services
{
    public class GlobalSettingsService : IGlobalSettingsService, IHostedService
    {
        private readonly ILogger<GlobalSettingsService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private ConcurrentDictionary<string, string> _cache = new();

        public GlobalSettingsService(ILogger<GlobalSettingsService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        // IHostedService implementace
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GlobalSettingsService starting. Initializing cache...");
            return InitializeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GlobalSettingsService stopping.");
            _cache.Clear();
            return Task.CompletedTask;
        }

        // Inicializace cache
        public async Task InitializeAsync()
        {
            await ReloadCacheAsync();
        }

        // Znovu načtení cache
        public async Task ReloadCacheAsync()
        {
            _logger.LogInformation("Reloading global settings cache...");
            var newCache = new ConcurrentDictionary<string, string>();

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var allSettings = await dbContext.GlobalSettings.AsNoTracking().ToListAsync();

                foreach (var setting in allSettings)
                {
                    newCache.TryAdd(setting.Key, setting.Value);
                }

                // Atomicky nahradit cache
                Interlocked.Exchange(ref _cache, newCache);

                _logger.LogInformation("Global settings cache reloaded successfully. Settings count: {Count}", newCache.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading global settings cache from database.");
            }
        }

        // Získání hodnot
        public string GetString(string key, string defaultValue = "")
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return value;
            }

            _logger.LogWarning("Global setting key '{Key}' not found. Using default value.", key);
            return defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (_cache.TryGetValue(key, out var value) && int.TryParse(value, out var intValue))
            {
                return intValue;
            }

            _logger.LogWarning("Global setting key '{Key}' not found or cannot be parsed as int. Using default value.", key);
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_cache.TryGetValue(key, out var value) && bool.TryParse(value, out var boolValue))
            {
                return boolValue;
            }

            _logger.LogWarning("Global setting key '{Key}' not found or cannot be parsed as bool. Using default value.", key);
            return defaultValue;
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            if (!_cache.TryGetValue(key, out var value))
            {
                _logger.LogWarning("Global setting key '{Key}' not found. Using default value.", key);
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert value '{Value}' to type {Type} for key '{Key}'. Using default value.", 
                    value, typeof(T).Name, key);
                return defaultValue;
            }
        }

        // Administrační metody
        public async Task<List<GlobalSetting>> GetAllSettingsAsync(
            string searchText = "", 
            string sortColumn = "Key", 
            string sortDirection = "asc", 
            int page = 1, 
            int pageSize = 20)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            IQueryable<GlobalSetting> query = dbContext.GlobalSettings;

            // Filtrování podle textu
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(s => s.Key.Contains(searchText) || 
                                        s.Value.Contains(searchText) || 
                                        (s.Description != null && s.Description.Contains(searchText)));
            }

            // Řazení
            query = sortColumn.ToLower() switch
            {
                "key" => sortDirection.ToLower() == "asc" 
                    ? query.OrderBy(s => s.Key) 
                    : query.OrderByDescending(s => s.Key),
                "value" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(s => s.Value)
                    : query.OrderByDescending(s => s.Value),
                "datatype" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(s => s.DataType)
                    : query.OrderByDescending(s => s.DataType),
                "createdat" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(s => s.CreatedAt)
                    : query.OrderByDescending(s => s.CreatedAt),
                "updatedat" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(s => s.UpdatedAt)
                    : query.OrderByDescending(s => s.UpdatedAt),
                _ => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(s => s.Key)
                    : query.OrderByDescending(s => s.Key) // Výchozí řazení podle klíče
            };

            // Stránkování
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalSettingsCountAsync(string searchText = "")
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            IQueryable<GlobalSetting> query = dbContext.GlobalSettings;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(s => s.Key.Contains(searchText) || 
                                        s.Value.Contains(searchText) || 
                                        (s.Description != null && s.Description.Contains(searchText)));
            }

            return await query.CountAsync();
        }

        public async Task<GlobalSetting?> GetSettingByIdAsync(int id)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await dbContext.GlobalSettings.FindAsync(id);
        }

        public async Task<GlobalSetting?> GetSettingByKeyAsync(string key)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await dbContext.GlobalSettings.FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task AddSettingAsync(GlobalSetting setting)
        {
            if (string.IsNullOrWhiteSpace(setting.Key))
            {
                throw new ArgumentException("Klíč nastavení nemůže být prázdný.");
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Kontrola, zda již klíč existuje
            bool exists = await dbContext.GlobalSettings.AnyAsync(s => s.Key == setting.Key);
            if (exists)
            {
                throw new InvalidOperationException($"Nastavení s klíčem '{setting.Key}' již existuje.");
            }

            setting.CreatedAt = DateTime.UtcNow;
            setting.UpdatedAt = DateTime.UtcNow;

            dbContext.GlobalSettings.Add(setting);
            await dbContext.SaveChangesAsync();
            await ReloadCacheAsync();
        }

        public async Task UpdateSettingAsync(GlobalSetting setting)
        {
            if (setting.Id <= 0)
            {
                throw new ArgumentException("Nelze aktualizovat nastavení bez platného ID.");
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Kontrola, zda již klíč existuje u jiného záznamu
            bool conflictingEntryExists = await dbContext.GlobalSettings
                .AnyAsync(s => s.Id != setting.Id && s.Key == setting.Key);

            if (conflictingEntryExists)
            {
                throw new InvalidOperationException($"Jiné nastavení s klíčem '{setting.Key}' již existuje.");
            }

            // Aktualizace časového razítka
            setting.UpdatedAt = DateTime.UtcNow;

            dbContext.Entry(setting).State = EntityState.Modified;
            
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Chyba při aktualizaci nastavení s ID {Id}", setting.Id);
                throw;
            }
            
            await ReloadCacheAsync();
        }

        public async Task DeleteSettingAsync(int id)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var setting = await dbContext.GlobalSettings.FindAsync(id);
            if (setting == null)
            {
                _logger.LogWarning("Pokus o smazání neexistujícího nastavení s ID {Id}", id);
                return; // Nothing to delete
            }

            dbContext.GlobalSettings.Remove(setting);
            await dbContext.SaveChangesAsync();
            await ReloadCacheAsync();
        }
    }
} 