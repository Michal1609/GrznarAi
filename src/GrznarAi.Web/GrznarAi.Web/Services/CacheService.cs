using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrznarAi.Web.Services
{
    public class CacheService : ICacheService, IHostedService
    {
        private readonly ILogger<CacheService> _logger;
        private readonly Timer _cleanupTimer;
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

        public CacheService(ILogger<CacheService> logger)
        {
            _logger = logger;
            // Spustit timer pro čištění expirovaných položek každých 5 minut
            _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Klíč nesmí být prázdný", nameof(key));

            // Zkusit získat z cache
            if (_cache.TryGetValue(key, out var cachedItem))
            {
                if (!cachedItem.IsExpired)
                {
                    _logger.LogDebug($"Cache hit pro klíč {key}");
                    return (T)cachedItem.Value;
                }
                else
                {
                    // Pokud je položka expirovaná, smažeme ji
                    _logger.LogDebug($"Cache položka expirovala pro klíč {key}");
                    _cache.TryRemove(key, out _);
                }
            }

            // Vytvořit novou položku
            _logger.LogDebug($"Cache miss pro klíč {key}, vytvářím novou položku");
            var value = await factory();
            
            if (value != null)
            {
                var newItem = new CacheItem
                {
                    Value = value,
                    Created = DateTime.UtcNow,
                    Expires = expirationTime.HasValue ? DateTime.UtcNow.Add(expirationTime.Value) : null,
                    Type = typeof(T).Name
                };
                
                // Vypočítat přibližnou velikost
                try
                {
                    var json = JsonSerializer.Serialize(value, _jsonOptions);
                    newItem.Size = json.Length * sizeof(char);
                }
                catch
                {
                    // Pokud nejde serializovat, použijeme odhad
                    newItem.Size = 1024;
                }
                
                _cache[key] = newItem;
            }
            
            return value;
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Klíč nesmí být prázdný", nameof(key));

            if (_cache.TryGetValue(key, out var cachedItem) && !cachedItem.IsExpired)
            {
                _logger.LogDebug($"Cache hit pro klíč {key}");
                return Task.FromResult((T)cachedItem.Value);
            }

            _logger.LogDebug($"Cache miss pro klíč {key}");
            return Task.FromResult<T>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Klíč nesmí být prázdný", nameof(key));

            if (value == null)
            {
                _logger.LogDebug($"Hodnota pro klíč {key} je null, odstraňuji z cache");
                _cache.TryRemove(key, out _);
                return Task.CompletedTask;
            }

            var newItem = new CacheItem
            {
                Value = value,
                Created = DateTime.UtcNow,
                Expires = expirationTime.HasValue ? DateTime.UtcNow.Add(expirationTime.Value) : null,
                Type = typeof(T).Name
            };
            
            // Vypočítat přibližnou velikost
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                newItem.Size = json.Length * sizeof(char);
            }
            catch
            {
                // Pokud nejde serializovat, použijeme odhad
                newItem.Size = 1024;
            }
            
            _cache[key] = newItem;
            _logger.LogDebug($"Nastavena cache pro klíč {key}, velikost: {newItem.Size} bytes");
            
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Klíč nesmí být prázdný", nameof(key));

            if (_cache.TryRemove(key, out _))
            {
                _logger.LogDebug($"Odstraněna položka z cache pro klíč {key}");
            }
            
            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            var count = _cache.Count;
            _cache.Clear();
            _logger.LogInformation($"Cache vyčištěna, odstraněno {count} položek");
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetAllKeysAsync()
        {
            return Task.FromResult(_cache.Keys.AsEnumerable());
        }

        public Task<IEnumerable<CacheItemInfo>> GetCacheInfoAsync()
        {
            var result = _cache.Select(kvp => new CacheItemInfo
            {
                Key = kvp.Key,
                Type = kvp.Value.Type,
                Created = kvp.Value.Created,
                Expires = kvp.Value.Expires,
                Size = kvp.Value.Size
            }).ToList();
            
            return Task.FromResult(result.AsEnumerable());
        }

        private void CleanupExpiredItems(object state)
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _cache.Where(kvp => kvp.Value.Expires.HasValue && kvp.Value.Expires.Value < now)
                                   .Select(kvp => kvp.Key)
                                   .ToList();

            if (expiredKeys.Any())
            {
                _logger.LogInformation($"Čištění expirovaných položek, nalezeno {expiredKeys.Count} expirovaných položek");
                
                foreach (var key in expiredKeys)
                {
                    _cache.TryRemove(key, out _);
                }
            }
        }

        // IHostedService implementace
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CacheService je spuštěn");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CacheService je zastavován");
            _cleanupTimer?.Change(Timeout.Infinite, 0);
            _cleanupTimer?.Dispose();
            return Task.CompletedTask;
        }

        private class CacheItem
        {
            public object Value { get; set; }
            public DateTime Created { get; set; }
            public DateTime? Expires { get; set; }
            public string Type { get; set; }
            public long Size { get; set; }
            
            public bool IsExpired => Expires.HasValue && Expires.Value < DateTime.UtcNow;
        }
    }
} 