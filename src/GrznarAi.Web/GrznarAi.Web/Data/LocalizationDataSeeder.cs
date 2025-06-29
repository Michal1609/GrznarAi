using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text.Json;

namespace GrznarAi.Web.Data
{
    public static class LocalizationDataSeeder
    {
        public static async Task SeedAsync(IDbContextFactory<ApplicationDbContext> contextFactory, string? jsonPath = null)
        {
            using var context = await contextFactory.CreateDbContextAsync();

            // Výchozí cesta k JSON souboru
            jsonPath ??= Path.Combine("wwwroot", "data", "localization-seed.json");
            if (!File.Exists(jsonPath))
                return;

            var json = await File.ReadAllTextAsync(jsonPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var seed = System.Text.Json.JsonSerializer.Deserialize<LocalizationSeed>(json, options);
            if (seed?.Localizations == null)
                return;

            foreach (var loc in seed.Localizations)
            {
                var entity = await context.LocalizationStrings.FirstOrDefaultAsync(l => l.Key == loc.Key && l.LanguageCode == loc.Language);
                if (entity != null)
                {
                    entity.Value = loc.Value;
                    entity.Description = loc.Description;
                }
                else
                {
                    context.LocalizationStrings.Add(new LocalizationString
                    {
                        Key = loc.Key,
                        LanguageCode = loc.Language,
                        Value = loc.Value,
                        Description = loc.Description
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        private class LocalizationSeed
        {
            public int Version { get; set; }
            public List<LocalizationSeedItem> Localizations { get; set; } = new();
        }
        private class LocalizationSeedItem
        {
            public string Language { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string? Description { get; set; }
        }
    }
} 