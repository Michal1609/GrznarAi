using System.Collections.Generic;
using System.Threading.Tasks;
using GrznarAi.Web.Data;

namespace GrznarAi.Web.Services
{
    public interface ILocalizationService
    {
        Task InitializeAsync(); // Method to load strings into cache
        string GetString(string key); // Method to get localized string for current UI culture
        string GetString(string key, string culture); // Method to get localized string for specific culture

        // Methods for administration
        Task<List<LocalizationString>> GetAllStringsAdminAsync();
        Task<LocalizationString?> GetSingleStringAdminAsync(int id); // Added: Get a specific entry by its Id for editing
        Task AddStringAsync(LocalizationString localizationString);
        Task UpdateStringAsync(LocalizationString localizationString);
        Task DeleteStringAsync(int id); // Id now refers to the specific language entry row
        Task ReloadCacheAsync(); // Method to reload cache after admin changes

        /// <summary>
        /// Nainstaluje/aktualizuje lokalizační stringy ze seedovacího JSON souboru.
        /// </summary>
        Task<bool> InstallFromJsonAsync(string? jsonPath = null);

        /// <summary>
        /// Smaže všechny lokalizační řetězce z databáze.
        /// </summary>
        Task DeleteAllStringsAsync();
    }
} 