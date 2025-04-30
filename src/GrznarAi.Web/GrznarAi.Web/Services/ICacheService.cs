using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface ICacheService
    {
        /// <summary>
        /// Získá položku z cache nebo spustí factory pro vytvoření nové položky pokud neexistuje
        /// </summary>
        /// <typeparam name="T">Typ objektu</typeparam>
        /// <param name="key">Klíč v cache</param>
        /// <param name="factory">Factory pro vytvoření objektu pokud není v cache</param>
        /// <param name="expirationTime">Doba platnosti v cache (null = bez expirace)</param>
        /// <returns>Objekt z cache nebo nově vytvořený</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null);
        
        /// <summary>
        /// Získá položku z cache
        /// </summary>
        /// <typeparam name="T">Typ objektu</typeparam>
        /// <param name="key">Klíč v cache</param>
        /// <returns>Objekt z cache nebo default(T) pokud nenalezen</returns>
        Task<T> GetAsync<T>(string key);
        
        /// <summary>
        /// Nastaví položku do cache
        /// </summary>
        /// <typeparam name="T">Typ objektu</typeparam>
        /// <param name="key">Klíč v cache</param>
        /// <param name="value">Hodnota k uložení</param>
        /// <param name="expirationTime">Doba platnosti v cache (null = bez expirace)</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null);
        
        /// <summary>
        /// Odstraní položku z cache
        /// </summary>
        /// <param name="key">Klíč v cache</param>
        Task RemoveAsync(string key);
        
        /// <summary>
        /// Vyčistí celou cache
        /// </summary>
        Task ClearAsync();
        
        /// <summary>
        /// Získá všechny klíče v cache
        /// </summary>
        Task<IEnumerable<string>> GetAllKeysAsync();
        
        /// <summary>
        /// Získá informace o položkách v cache
        /// </summary>
        Task<IEnumerable<CacheItemInfo>> GetCacheInfoAsync();
    }

    public class CacheItemInfo
    {
        /// <summary>
        /// Klíč položky v cache
        /// </summary>
        public string Key { get; set; }
        
        /// <summary>
        /// Typ objektu v cache
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Čas vytvoření/aktualizace položky
        /// </summary>
        public DateTime Created { get; set; }
        
        /// <summary>
        /// Čas expirace položky (null = bez expirace)
        /// </summary>
        public DateTime? Expires { get; set; }
        
        /// <summary>
        /// Velikost položky v cache (přibližná)
        /// </summary>
        public long Size { get; set; }
    }
} 