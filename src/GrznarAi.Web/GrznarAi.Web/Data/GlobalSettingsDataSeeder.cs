using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrznarAi.Web.Data
{
    public static class GlobalSettingsDataSeeder
    {
        public static async Task SeedAsync(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger logger)
        {
            logger.LogInformation("Inicializace výchozích globálních nastavení...");
            
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
            
            // Nastavení pro AI News
            AddSetting("AiNews.DuplicateCheckDays", "10", "int", "Počet dní pro kontrolu duplicit při importu AI novinek");
            
            if (settings.Count > 0)
            {
                await context.GlobalSettings.AddRangeAsync(settings);
                await context.SaveChangesAsync();
                logger.LogInformation("Inicializováno {Count} globálních nastavení", settings.Count);
            }
            else
            {
                logger.LogInformation("Všechna globální nastavení již existují, žádná nová nebyla přidána");
            }
        }
    }
} 