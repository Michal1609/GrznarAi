using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Univerzální HostedService pro spouštění úloh v pozadí.
    /// </summary>
    public class BackgroundTaskService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundTaskService> _logger;
        private readonly List<(string Name, Func<IServiceProvider, Task> Action, TimeSpan Interval, DateTime LastRun)> _tasks;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Kontrola co minutu
        
        // Veřejná property pro zapnutí/vypnutí stahování dat z Ecowitt API
        public bool IsEcowittDataFetchEnabled 
        { 
            get 
            {
                // Vytvoříme scope pro získání IGlobalSettingsService
                using var scope = _serviceProvider.CreateScope();
                var globalSettingsService = scope.ServiceProvider.GetRequiredService<IGlobalSettingsService>();
                var result = globalSettingsService.GetBool("Weather.IsEcowittApiEnabled", false);
                _logger.LogDebug("IsEcowittDataFetchEnabled getter: hodnota je {value}", result);
                return result;
            }
            set 
            {
                _logger.LogInformation("Nastavení IsEcowittDataFetchEnabled na {value}", value);
                using var scope = _serviceProvider.CreateScope();
                var globalSettingsService = scope.ServiceProvider.GetRequiredService<IGlobalSettingsService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                var setting = dbContext.GlobalSettings.FirstOrDefault(s => s.Key == "Weather.IsEcowittApiEnabled");
                if (setting == null)
                {
                    _logger.LogInformation("Vytvářím nové nastavení Weather.IsEcowittApiEnabled");
                    setting = new Data.GlobalSetting
                    {
                        Key = "Weather.IsEcowittApiEnabled",
                        Value = value.ToString().ToLower(),
                        DataType = "bool",
                        Description = "Povolení automatického stahování dat z Ecowitt API",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    dbContext.GlobalSettings.Add(setting);
                }
                else
                {
                    _logger.LogInformation("Aktualizuji existující nastavení Weather.IsEcowittApiEnabled z {oldValue} na {newValue}", 
                        setting.Value, value.ToString().ToLower());
                    setting.Value = value.ToString().ToLower();
                    setting.UpdatedAt = DateTime.UtcNow;
                }
                dbContext.SaveChanges();
                
                // Aktualizace cache v GlobalSettingsService - použijeme blokující volání, aby byla cache aktualizována ihned
                // Pozor, toto je potenciálně nebezpečné a může způsobit deadlock, ale v tomto případě potřebujeme
                // zajistit, že cache bude aktualizována před dalším použitím property
                globalSettingsService.ReloadCacheAsync().GetAwaiter().GetResult();
                
                // Ověříme, zda byla hodnota skutečně uložena a aktualizována v cache
                var newValue = globalSettingsService.GetBool("Weather.IsEcowittApiEnabled", false);
                _logger.LogInformation("Hodnota nastavení po aktualizaci cache: {value}", newValue);
                
                _logger.LogInformation("Stahování dat z Ecowitt API bylo {0}", value ? "povoleno" : "zakázáno");
            }
        }

        public BackgroundTaskService(
            IServiceProvider serviceProvider,
            ILogger<BackgroundTaskService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Definujeme úlohy, které chceme spouštět
            _tasks = new List<(string Name, Func<IServiceProvider, Task> Action, TimeSpan Interval, DateTime LastRun)>
            {
                // Každých 10 minut stahujeme data z Ecowitt API
                ("FetchEcowittData", async (provider) => 
                {
                    // Vytvoříme scope pro získání služeb
                    using var scope = provider.CreateScope();
                    var globalSettingsService = scope.ServiceProvider.GetRequiredService<IGlobalSettingsService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                    
                    // Přečteme hodnotu nastavení přímo z databáze, aby byla vždy aktuální
                    var settingFromDb = await dbContext.GlobalSettings.FirstOrDefaultAsync(s => s.Key == "Weather.IsEcowittApiEnabled");
                    bool isEnabled = settingFromDb != null && bool.TryParse(settingFromDb.Value, out var parsed) && parsed;
                    
                    // Pro porovnání získáme také hodnotu z cache
                    var isEnabledFromCache = globalSettingsService.GetBool("Weather.IsEcowittApiEnabled", false);
                    
                    // Zalogujeme oba hodnoty, aby bylo možné zjistit případný problém s nekonzistencí
                    _logger.LogInformation("Kontrola nastavení Weather.IsEcowittApiEnabled. Z databáze: {dbValue}, z cache: {cacheValue}", 
                        isEnabled, isEnabledFromCache);
                    
                    // Pokud se hodnoty liší, aktualizujeme cache
                    if (isEnabled != isEnabledFromCache)
                    {
                        _logger.LogWarning("Nekonzistence mezi hodnotou v databázi a cache, aktualizuji cache");
                        await globalSettingsService.ReloadCacheAsync();
                    }
                    
                    // Přeskočit, pokud je funkce vypnutá v databázi
                    if (!isEnabled)
                    {
                        _logger.LogInformation("Stahování dat z Ecowitt API je zakázáno. Úloha přeskočena.");
                        return;
                    }
                
                    _logger.LogInformation("Zahajuji stahování dat z Ecowitt API");
                    var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherHistoryService>();
                    await weatherService.FetchAndStoreEcowittDataAsync();
                    _logger.LogInformation("Stahování dat z Ecowitt API dokončeno");
                }, TimeSpan.FromMinutes(10), DateTime.MinValue),

                // Další úlohy lze přidat sem - např. odesílání emailů v dávkách, atd.
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundTaskService byl spuštěn");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                // Zkontrolujeme všechny úlohy
                for (int i = 0; i < _tasks.Count; i++)
                {
                    var task = _tasks[i];
                    
                    // Pokud nastal čas pro spuštění úlohy
                    if ((now - task.LastRun) >= task.Interval)
                    {
                        // Výpis aktuálního nastavení před rozhodnutím o spuštění úlohy
                        if (task.Name == "FetchEcowittData")
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var globalSettingsService = scope.ServiceProvider.GetRequiredService<IGlobalSettingsService>();
                            var isEnabled = globalSettingsService.GetBool("Weather.IsEcowittApiEnabled", false);
                            _logger.LogInformation("Před spuštěním úlohy FetchEcowittData, hodnota Weather.IsEcowittApiEnabled: {value}", isEnabled);
                        }
                        
                        try
                        {
                            _logger.LogInformation("Spouštění úlohy: {TaskName}", task.Name);
                            await task.Action(_serviceProvider);
                            
                            // Aktualizujeme čas posledního spuštění
                            _tasks[i] = (task.Name, task.Action, task.Interval, now);
                            
                            _logger.LogInformation("Úloha dokončena: {TaskName}", task.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Chyba při spouštění úlohy: {TaskName}", task.Name);
                            
                            // I v případě chyby aktualizujeme čas posledního spuštění,
                            // abychom zabránili opakovaným selháním
                            _tasks[i] = (task.Name, task.Action, task.Interval, now);
                        }
                    }
                }

                // Počkáme na další kontrolu
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
} 