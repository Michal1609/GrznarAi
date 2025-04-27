using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GrznarAi.Web.Api.Models;
using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GrznarAi.Web.Tools
{
    public class ApiKeyGenerator
    {
        /// <summary>
        /// Vygeneruje a přidá nový API klíč do databáze
        /// </summary>
        public static async Task GenerateApiKey(IHost app, string name, string description = "Default API key", int? expiresInDays = null)
        {
            // Získání služby DbContext
            using var scope = app.Services.CreateScope();
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = await contextFactory.CreateDbContextAsync();

            // Vygenerování API klíče (32 bytů = 256 bitů)
            var keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            var keyValue = Convert.ToBase64String(keyBytes);

            // Vytvoření entity API klíče
            var apiKey = new ApiKey
            {
                Name = name,
                Value = keyValue,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ExpiresAt = expiresInDays.HasValue ? DateTime.UtcNow.AddDays(expiresInDays.Value) : null
            };

            // Přidání do databáze
            context.ApiKeys.Add(apiKey);
            await context.SaveChangesAsync();

            // Výpis výsledku
            Console.WriteLine("API klíč byl úspěšně vygenerován a přidán do databáze:");
            Console.WriteLine($"Název: {apiKey.Name}");
            Console.WriteLine($"Hodnota: {apiKey.Value}");
            Console.WriteLine($"Popis: {apiKey.Description}");
            Console.WriteLine($"Vytvořen: {apiKey.CreatedAt}");
            Console.WriteLine($"Platný do: {apiKey.ExpiresAt?.ToString() ?? "Neomezeně"}");
        }
    }
} 