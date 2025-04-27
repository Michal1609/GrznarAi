using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public class AiNewsService : IAiNewsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AiNewsService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<(List<AiNewsItem> Items, int TotalCount)> GetAiNewsAsync(int page = 1, int pageSize = 20, string searchTerm = null, int? year = null, int? month = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Základní dotaz
            var query = context.AiNewsItems
                .Where(n => n.IsActive)
                .AsQueryable();
            
            // Aplikace vyhledávání, pokud je zadán searchTerm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(n => 
                    n.TitleEn.ToLower().Contains(searchTerm) || 
                    n.TitleCz.ToLower().Contains(searchTerm) || 
                    n.SummaryEn.ToLower().Contains(searchTerm) ||
                    n.SummaryCz.ToLower().Contains(searchTerm) ||
                    n.ContentEn.ToLower().Contains(searchTerm) ||
                    n.ContentCz.ToLower().Contains(searchTerm));
            }
            
            // Filtrování podle roku a měsíce
            if (year.HasValue)
            {
                query = query.Where(n => n.PublishedDate.HasValue && n.PublishedDate.Value.Year == year.Value);
                
                if (month.HasValue)
                {
                    query = query.Where(n => n.PublishedDate.HasValue && n.PublishedDate.Value.Month == month.Value);
                }
            }
            
            // Získání celkového počtu výsledků
            var totalCount = await query.CountAsync();
            
            // Aplikace řazení a stránkování
            var items = await query
                .OrderByDescending(n => n.PublishedDate ?? n.ImportedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return (items, totalCount);
        }

        public async Task<AiNewsItem> GetAiNewsItemByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AiNewsItems.FindAsync(id);
        }

        public async Task<AiNewsItem> AddAiNewsItemAsync(AiNewsItem newsItem)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            newsItem.ImportedDate = DateTime.UtcNow;
            
            context.AiNewsItems.Add(newsItem);
            await context.SaveChangesAsync();
            
            return newsItem;
        }

        public async Task<bool> UpdateAiNewsItemAsync(AiNewsItem newsItem)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var existingItem = await context.AiNewsItems.FindAsync(newsItem.Id);
            
            if (existingItem == null)
            {
                return false;
            }
            
            // Aktualizace všech vlastností kromě ImportedDate
            existingItem.TitleEn = newsItem.TitleEn;
            existingItem.TitleCz = newsItem.TitleCz;
            existingItem.ContentEn = newsItem.ContentEn;
            existingItem.ContentCz = newsItem.ContentCz;
            existingItem.SummaryEn = newsItem.SummaryEn;
            existingItem.SummaryCz = newsItem.SummaryCz;
            existingItem.Url = newsItem.Url;
            existingItem.ImageUrl = newsItem.ImageUrl;
            existingItem.SourceName = newsItem.SourceName;
            existingItem.PublishedDate = newsItem.PublishedDate;
            existingItem.IsActive = newsItem.IsActive;
            
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAiNewsItemAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var newsItem = await context.AiNewsItems.FindAsync(id);
            
            if (newsItem == null)
            {
                return false;
            }
            
            context.AiNewsItems.Remove(newsItem);
            await context.SaveChangesAsync();
            
            return true;
        }

        public async Task<List<(int Year, int Month, int Count)>> GetArchiveMonthsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Získáme všechny aktivní novinky s platným datem publikování
            var newsWithDates = await context.AiNewsItems
                .Where(n => n.IsActive && n.PublishedDate.HasValue)
                .Select(n => new { 
                    Year = n.PublishedDate.Value.Year,
                    Month = n.PublishedDate.Value.Month
                })
                .ToListAsync();
            
            // Seskupíme podle roku a měsíce a spočítáme počet článků
            var archives = newsWithDates
                .GroupBy(n => new { n.Year, n.Month })
                .Select(g => (Year: g.Key.Year, Month: g.Key.Month, Count: g.Count()))
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();
                
            return archives;
        }

        /// <summary>
        /// Přidá seznam nových AI novinek
        /// </summary>
        /// <param name="newsItems">Seznam nových AI novinek</param>
        /// <returns>Počet přidaných novinek</returns>
        public async Task<int> AddAiNewsItemsAsync(List<AiNewsItem> newsItems)
        {
            if (newsItems == null || !newsItems.Any())
            {
                return 0;
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Nastavit datum importu pro všechny položky
            var now = DateTime.UtcNow;
            foreach (var item in newsItems)
            {
                item.ImportedDate = now;
            }
            
            // Přidat všechny položky
            context.AiNewsItems.AddRange(newsItems);
            
            // Uložit změny
            return await context.SaveChangesAsync();
        }
    }
} 