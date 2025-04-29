using System.Collections.Generic;
using System.Threading.Tasks;
using GrznarAi.Web.Data;

namespace GrznarAi.Web.Services
{
    public interface IGlobalSettingsService
    {
        Task InitializeAsync(); // Metoda pro inicializaci cache při startu
        
        // Metody pro získání hodnot
        string GetString(string key, string defaultValue = "");
        int GetInt(string key, int defaultValue = 0);
        bool GetBool(string key, bool defaultValue = false);
        
        // Metody pro administraci
        Task<List<GlobalSetting>> GetAllSettingsAsync(
            string searchText = "", 
            string sortColumn = "Key", 
            string sortDirection = "asc", 
            int page = 1, 
            int pageSize = 20);
        Task<int> GetTotalSettingsCountAsync(string searchText = "");
        Task<GlobalSetting?> GetSettingByIdAsync(int id);
        Task<GlobalSetting?> GetSettingByKeyAsync(string key);
        Task AddSettingAsync(GlobalSetting setting);
        Task UpdateSettingAsync(GlobalSetting setting);
        Task DeleteSettingAsync(int id);
        Task ReloadCacheAsync(); // Metoda pro znovu načtení cache po změnách
        
        // Pomocné metody pro konverze
        T GetValue<T>(string key, T defaultValue);
    }
} 