using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public class AiNewsSourceService : IAiNewsSourceService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AiNewsSourceService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<(List<AiNewsSource> Items, int TotalCount)> GetSourcesAsync(int page = 1, int pageSize = 20, string searchTerm = null, SourceType? type = null, bool activeOnly = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Základní dotaz
            var query = context.AiNewsSources.AsQueryable();
            
            // Filtrování podle aktivního stavu
            if (activeOnly)
            {
                query = query.Where(s => s.IsActive);
            }
            
            // Aplikace vyhledávání, pokud je zadán searchTerm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(s => 
                    s.Name.ToLower().Contains(searchTerm) || 
                    s.Url.ToLower().Contains(searchTerm) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchTerm)));
            }
            
            // Filtrování podle typu zdroje
            if (type.HasValue)
            {
                query = query.Where(s => s.Type == type.Value);
            }
            
            // Získání celkového počtu výsledků
            var totalCount = await query.CountAsync();
            
            // Aplikace řazení a stránkování
            var items = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return (items, totalCount);
        }

        public async Task<AiNewsSource> GetSourceByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AiNewsSources.FindAsync(id);
        }

        public async Task<AiNewsSource> AddSourceAsync(AiNewsSource source)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            source.CreatedAt = DateTime.UtcNow;
            source.UpdatedAt = DateTime.UtcNow;
            
            context.AiNewsSources.Add(source);
            await context.SaveChangesAsync();
            
            return source;
        }

        public async Task<bool> UpdateSourceAsync(AiNewsSource source)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var existingSource = await context.AiNewsSources.FindAsync(source.Id);
            
            if (existingSource == null)
            {
                return false;
            }
            
            // Aktualizace všech vlastností s výjimkou CreatedAt
            existingSource.Name = source.Name;
            existingSource.Url = source.Url;
            existingSource.Type = source.Type;
            existingSource.IsActive = source.IsActive;
            existingSource.Description = source.Description;
            existingSource.Parameters = source.Parameters;
            existingSource.UpdatedAt = DateTime.UtcNow;
            
            // LastFetched neaktualizujeme tady - na to je speciální metoda
            
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSourceAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var source = await context.AiNewsSources.FindAsync(id);
            
            if (source == null)
            {
                return false;
            }
            
            context.AiNewsSources.Remove(source);
            await context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> UpdateLastFetchedAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var source = await context.AiNewsSources.FindAsync(id);
            
            if (source == null)
            {
                return false;
            }
            
            source.LastFetched = DateTime.UtcNow;
            source.UpdatedAt = DateTime.UtcNow;
            
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AiNewsSource>> GetActiveSourcesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AiNewsSources
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        
        /// <summary>
        /// Hromadně aktualizuje datum posledního stažení pro zdroje odpovídající zadaným URL
        /// </summary>
        /// <param name="sourceUrls">Seznam URL zdrojů, u kterých aktualizovat LastFetched</param>
        /// <returns>Počet aktualizovaných zdrojů</returns>
        public async Task<int> UpdateLastFetchedBulkAsync(IEnumerable<string> sourceUrls)
        {
            if (sourceUrls == null || !sourceUrls.Any())
            {
                return 0;
            }
            
            // Převedeme na list a odstraníme duplicity
            var urlList = sourceUrls.Where(url => !string.IsNullOrEmpty(url)).Distinct().ToList();
            
            if (!urlList.Any())
            {
                return 0;
            }
            
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Získáme všechny zdroje, jejichž URL je v seznamu
            var sources = await context.AiNewsSources
                .Where(s => urlList.Contains(s.Url))
                .ToListAsync();
            
            if (!sources.Any())
            {
                return 0;
            }
            
            var now = DateTime.UtcNow;
            
            // Aktualizujeme datum posledního stažení pro všechny nalezené zdroje
            foreach (var source in sources)
            {
                source.LastFetched = now;
            }
            
            // Uložíme změny a vrátíme počet aktualizovaných zdrojů
            return await context.SaveChangesAsync();
        }
    }
} 