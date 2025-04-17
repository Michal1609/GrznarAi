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
        Task<LocalizationString?> GetStringByIdAsync(int id);
        Task AddStringAsync(LocalizationString localizationString);
        Task UpdateStringAsync(LocalizationString localizationString);
        Task DeleteStringAsync(int id);
        Task ReloadCacheAsync(); // Method to reload cache after admin changes
    }
} 