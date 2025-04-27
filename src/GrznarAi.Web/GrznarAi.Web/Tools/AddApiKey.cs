using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GrznarAi.Web.Api.Models;
using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Tools
{
    public class AddApiKey
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AddApiKey(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Vygeneruje nový API klíč a přidá ho do databáze
        /// </summary>
        /// <param name="name">Název API klíče</param>
        /// <param name="description">Popis API klíče</param>
        /// <param name="expiresInDays">Počet dní platnosti (null = neomezená platnost)</param>
        /// <returns>Vygenerovaný API klíč</returns>
        public async Task<ApiKey> GenerateAndAddApiKeyAsync(string name, string? description = null, int? expiresInDays = null)
        {
            // Vygenerovat API klíč (32 bytů = 256 bitů)
            var keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            var keyValue = Convert.ToBase64String(keyBytes);

            // Vytvořit entitu API klíče
            var apiKey = new ApiKey
            {
                Name = name,
                Value = keyValue,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ExpiresAt = expiresInDays.HasValue ? DateTime.UtcNow.AddDays(expiresInDays.Value) : null
            };

            // Přidat do databáze
            using var context = await _contextFactory.CreateDbContextAsync();
            context.ApiKeys.Add(apiKey);
            await context.SaveChangesAsync();

            return apiKey;
        }
    }
} 