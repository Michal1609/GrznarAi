using GrznarAi.Web.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface IAiNewsSourceService
    {
        /// <summary>
        /// Získá seznam všech zdrojů AI novinek s možností filtrování a paginace
        /// </summary>
        /// <param name="page">Číslo stránky (začíná od 1)</param>
        /// <param name="pageSize">Počet položek na stránku</param>
        /// <param name="searchTerm">Vyhledávaný text (nepovinný)</param>
        /// <param name="type">Typ zdroje pro filtrování (nepovinný)</param>
        /// <param name="activeOnly">Zda má vrátit pouze aktivní zdroje</param>
        /// <returns>Seznam zdrojů AI novinek s celkovým počtem</returns>
        Task<(List<AiNewsSource> Items, int TotalCount)> GetSourcesAsync(int page = 1, int pageSize = 20, string searchTerm = null, SourceType? type = null, bool activeOnly = false);

        /// <summary>
        /// Získá konkrétní zdroj AI novinek podle ID
        /// </summary>
        /// <param name="id">ID zdroje</param>
        /// <returns>Zdroj AI novinek nebo null, pokud nebyl nalezen</returns>
        Task<AiNewsSource> GetSourceByIdAsync(int id);

        /// <summary>
        /// Přidá nový zdroj AI novinek
        /// </summary>
        /// <param name="source">Nový zdroj AI novinek</param>
        /// <returns>Přidaný zdroj s vygenerovaným ID</returns>
        Task<AiNewsSource> AddSourceAsync(AiNewsSource source);

        /// <summary>
        /// Aktualizuje existující zdroj AI novinek
        /// </summary>
        /// <param name="source">Aktualizovaný zdroj AI novinek</param>
        /// <returns>True, pokud byl zdroj úspěšně aktualizován</returns>
        Task<bool> UpdateSourceAsync(AiNewsSource source);

        /// <summary>
        /// Odstraní zdroj AI novinek podle ID
        /// </summary>
        /// <param name="id">ID zdroje</param>
        /// <returns>True, pokud byl zdroj úspěšně odstraněn</returns>
        Task<bool> DeleteSourceAsync(int id);

        /// <summary>
        /// Aktualizuje čas posledního stažení pro daný zdroj
        /// </summary>
        /// <param name="id">ID zdroje</param>
        /// <returns>True, pokud bylo datum úspěšně aktualizováno</returns>
        Task<bool> UpdateLastFetchedAsync(int id);

        /// <summary>
        /// Získá seznam všech aktivních zdrojů AI novinek (pro API)
        /// </summary>
        /// <returns>Seznam aktivních zdrojů AI novinek</returns>
        Task<List<AiNewsSource>> GetActiveSourcesAsync();
        
        /// <summary>
        /// Hromadně aktualizuje datum posledního stažení pro zdroje odpovídající zadaným URL
        /// </summary>
        /// <param name="sourceUrls">Seznam URL zdrojů, u kterých aktualizovat LastFetched</param>
        /// <returns>Počet aktualizovaných zdrojů</returns>
        Task<int> UpdateLastFetchedBulkAsync(IEnumerable<string> sourceUrls);
    }
} 