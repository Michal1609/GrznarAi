using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    public class GlobalSettingsDataSeeder
    {
        public static async Task SeedAsync(IDbContextFactory<ApplicationDbContext> contextFactory)
        {            
            using var context = await contextFactory.CreateDbContextAsync();
            var settings = new List<GlobalSetting>();
            var existingKeys = new HashSet<string>(
                await context.GlobalSettings
                    .AsNoTracking()
                    .Select(s => s.Key)
                    .ToListAsync());

            // Pomocná funkce pro přidání nastavení
            void AddSetting(string key, string value, string dataType, string description)
            {
                if (!existingKeys.Contains(key))
                {
                    settings.Add(new GlobalSetting
                    {
                        Key = key,
                        Value = value,
                        DataType = dataType,
                        Description = description,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            // Pouze jedno nastavení - Admin.GlobalSettings.PageSize
            AddSetting("Admin.GlobalSettings.PageSize", "50", "int", "Počet položek na stránku v administraci globálních nastavení");
            
            // Nastavení pro BroadcastAnnouncements
            AddSetting("BroadcastAnnouncements.PageSize", "10", "int", "Počet hlášení rozhlasu zobrazených na jedné stránce.");
            
            // Nastavení pro AI News
            AddSetting("AiNews.DuplicateCheckDays", "10", "int", "Počet dní pro kontrolu duplicit při importu AI novinek");
            
            if (settings.Count > 0)
            {
                await context.GlobalSettings.AddRangeAsync(settings);
                await context.SaveChangesAsync();                
            }
        }
    }
} 