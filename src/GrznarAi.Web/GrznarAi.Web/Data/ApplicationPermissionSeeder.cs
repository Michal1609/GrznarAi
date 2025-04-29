using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Data
{
    public static class ApplicationPermissionSeeder
    {
        public static async Task SeedAsync(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            using var context = await contextFactory.CreateDbContextAsync();
            
            var permissions = new List<ApplicationPermission>();
            var existingKeys = new HashSet<string>(
                await context.ApplicationPermissions
                    .AsNoTracking()
                    .Select(p => p.Key)
                    .ToListAsync());

            // Helper function to add permissions if not already present
            void AddPermission(string key, string name, string? description)
            {
                if (!existingKeys.Contains(key))
                {
                    permissions.Add(new ApplicationPermission
                    {
                        Key = key,
                        Name = name,
                        Description = description,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            // --- Permissions Seed Data ---
            
            // Oprávnění pro aplikaci Poznámky
            AddPermission("App.Notes", "Aplikace Poznámky", "Povoluje přístup k aplikaci Poznámky");
            
            // Zde mohou být přidána další oprávnění

            // --- End Permissions Seed Data ---

            try
            {
                // Přidání pouze nových oprávnění do databáze
                if (permissions.Any())
                {
                    await context.ApplicationPermissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Přidáno {permissions.Count} nových oprávnění do databáze.");
                }
                else
                {
                    Console.WriteLine("Všechna oprávnění již existují, žádná nová nebyla přidána.");
                }
            }
            catch (Exception ex)
            {
                // Logování chyby
                Console.WriteLine($"Chyba při seedování oprávnění: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Vnitřní chyba: {ex.InnerException.Message}");
                }
                throw; // Předání chyby dál pro zpracování
            }
        }
    }
} 