using GrznarAi.Web.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface IAiNewsService
    {
        /// <summary>
        /// Získá seznam AI novinek s paginací
        /// </summary>
        /// <param name="page">Číslo stránky (začíná od 1)</param>
        /// <param name="pageSize">Počet položek na stránku</param>
        /// <param name="searchTerm">Vyhledávaný text (nepovinný)</param>
        /// <param name="year">Filtrovat podle roku (nepovinné)</param>
        /// <param name="month">Filtrovat podle měsíce (nepovinné)</param>
        /// <returns>Seznam AI novinek</returns>
        Task<(List<AiNewsItem> Items, int TotalCount)> GetAiNewsAsync(int page = 1, int pageSize = 20, string searchTerm = null, int? year = null, int? month = null);
        
        /// <summary>
        /// Získá konkrétní AI novinku podle ID
        /// </summary>
        /// <param name="id">ID novinky</param>
        /// <returns>AI novinka nebo null, pokud nebyla nalezena</returns>
        Task<AiNewsItem> GetAiNewsItemByIdAsync(int id);
        
        /// <summary>
        /// Přidá novou AI novinku
        /// </summary>
        /// <param name="newsItem">Nová AI novinka</param>
        /// <returns>Přidaná AI novinka s vygenerovaným ID</returns>
        Task<AiNewsItem> AddAiNewsItemAsync(AiNewsItem newsItem);
        
        /// <summary>
        /// Aktualizuje existující AI novinku
        /// </summary>
        /// <param name="newsItem">Aktualizovaná AI novinka</param>
        /// <returns>True, pokud byla novinka úspěšně aktualizována</returns>
        Task<bool> UpdateAiNewsItemAsync(AiNewsItem newsItem);
        
        /// <summary>
        /// Odstraní AI novinku podle ID
        /// </summary>
        /// <param name="id">ID novinky</param>
        /// <returns>True, pokud byla novinka úspěšně odstraněna</returns>
        Task<bool> DeleteAiNewsItemAsync(int id);

        /// <summary>
        /// Získá seznam dostupných roků a měsíců s počtem článků
        /// </summary>
        /// <returns>Seznam roků a měsíců s počtem článků</returns>
        Task<List<(int Year, int Month, int Count)>> GetArchiveMonthsAsync();

        /// <summary>
        /// Přidá seznam nových AI novinek
        /// </summary>
        /// <param name="newsItems">Seznam nových AI novinek</param>
        /// <returns>Počet přidaných novinek</returns>
        Task<int> AddAiNewsItemsAsync(List<AiNewsItem> newsItems);
    }
} 