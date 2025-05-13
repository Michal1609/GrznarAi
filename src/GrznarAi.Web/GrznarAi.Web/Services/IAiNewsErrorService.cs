using GrznarAi.Web.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface IAiNewsErrorService
    {
        /// <summary>
        /// Získá seznam chyb při stahování AI novinek s možností filtrování a paginace
        /// </summary>
        /// <param name="page">Číslo stránky (začíná od 1)</param>
        /// <param name="pageSize">Počet položek na stránku</param>
        /// <param name="searchTerm">Vyhledávaný text (nepovinný)</param>
        /// <param name="sourceId">ID zdroje pro filtrování (nepovinný)</param>
        /// <param name="category">Kategorie chyb pro filtrování (nepovinný)</param>
        /// <param name="isResolved">Zda má zahrnout vyřešené chyby</param>
        /// <returns>Seznam chyb s celkovým počtem</returns>
        Task<List<AiNewsError>> GetErrorsAsync(int page, int pageSize, string searchTerm = null, int? sourceId = null, string category = null, bool? isResolved = null);

        /// <summary>
        /// Získá celkový počet chyb při stahování AI novinek s možností filtrování
        /// </summary>
        /// <param name="searchTerm">Vyhledávaný text (nepovinný)</param>
        /// <param name="sourceId">ID zdroje pro filtrování (nepovinný)</param>
        /// <param name="category">Kategorie chyb pro filtrování (nepovinný)</param>
        /// <param name="isResolved">Zda má zahrnout vyřešené chyby</param>
        /// <returns>Celkový počet chyb</returns>
        Task<int> GetErrorsCountAsync(string searchTerm = null, int? sourceId = null, string category = null, bool? isResolved = null);

        /// <summary>
        /// Získá konkrétní chybu podle ID
        /// </summary>
        /// <param name="id">ID chyby</param>
        /// <returns>Chyba nebo null, pokud nebyla nalezena</returns>
        Task<AiNewsError> GetErrorByIdAsync(int id);

        /// <summary>
        /// Přidá nový záznam o chybě
        /// </summary>
        /// <param name="error">Nová chyba</param>
        /// <returns>Přidaná chyba s vygenerovaným ID</returns>
        Task<int> AddErrorAsync(AiNewsError error);

        /// <summary>
        /// Přidá kolekci záznamů o chybách (pro hromadný import)
        /// </summary>
        /// <param name="errors">Kolekce chyb</param>
        /// <returns>Počet úspěšně přidaných chyb</returns>
        Task AddErrorsAsync(IEnumerable<AiNewsError> errors);

        /// <summary>
        /// Označí chybu jako vyřešenou
        /// </summary>
        /// <param name="id">ID chyby</param>
        /// <param name="resolution">Poznámka k řešení (nepovinná)</param>
        /// <returns>True, pokud byla chyba úspěšně označena jako vyřešená</returns>
        Task<bool> MarkAsResolvedAsync(int id, string resolution);

        /// <summary>
        /// Smaže chybu podle ID
        /// </summary>
        /// <param name="id">ID chyby</param>
        /// <returns>True, pokud byla chyba úspěšně smazána</returns>
        Task<bool> DeleteErrorAsync(int id);

        /// <summary>
        /// Smaže všechny vyřešené chyby
        /// </summary>
        /// <returns>Počet smazaných chyb</returns>
        Task<int> DeleteResolvedErrorsAsync();
        
        /// <summary>
        /// Smaže všechny chyby v databázi
        /// </summary>
        /// <returns>Počet smazaných chyb</returns>
        Task<int> DeleteAllErrorsAsync();

        /// <summary>
        /// Získá počet nevyřešených chyb
        /// </summary>
        /// <returns>Počet nevyřešených chyb</returns>
        Task<int> GetUnresolvedErrorsCountAsync();
    }
} 