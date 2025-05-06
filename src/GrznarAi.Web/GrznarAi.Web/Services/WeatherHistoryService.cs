using GrznarAi.Web.Data;
using GrznarAi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public class EcowittApiSettings
    {
        public string ApiUrl { get; set; } = "https://api.ecowitt.net/api/v3/device/history";
        public string ApplicationKey { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
    }

    public interface IWeatherHistoryService
    {
        Task<DateTime?> GetLastRecordDateAsync();
        Task<List<WeatherHistory>> GetHistoryAsync(DateTime startDate, DateTime endDate);
        Task ImportCsvDataAsync(string csvContent);
        Task FetchAndStoreEcowittDataAsync();
        Task FetchAndStoreEcowittDataForPeriodAsync(DateTime startDate);
    }

    public class WeatherHistoryService : IWeatherHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WeatherHistoryService> _logger;
        private readonly IGlobalSettingsService _globalSettingsService;
        private readonly EcowittApiSettings _ecowittSettings;

        public WeatherHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IHttpClientFactory httpClientFactory,
            ILogger<WeatherHistoryService> logger,
            IGlobalSettingsService globalSettingsService,
            IOptions<EcowittApiSettings> ecowittSettings)
        {
            _contextFactory = contextFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _globalSettingsService = globalSettingsService;
            _ecowittSettings = ecowittSettings.Value;
        }

        public async Task<DateTime?> GetLastRecordDateAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.WeatherHistory
                .OrderByDescending(h => h.Date)
                .Select(h => h.Date)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WeatherHistory>> GetHistoryAsync(DateTime startDate, DateTime endDate)
        {
            // Omezení maximálního rozsahu dat na 90 dnů, aby se zabránilo přetížení paměti
            var maxAllowedPeriod = TimeSpan.FromDays(90);
            if (endDate - startDate > maxAllowedPeriod)
            {
                _logger.LogWarning("Požadovaný interval je příliš velký ({RequestedDays} dnů). Omezeno na maximální povolený interval ({MaxDays} dnů).", 
                    (endDate - startDate).TotalDays, maxAllowedPeriod.TotalDays);
                endDate = startDate.Add(maxAllowedPeriod);
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Omezení počtu vrácených záznamů na 10 000 pro zabránění zahlcení paměti
            const int MAX_RECORDS = 10000;
            
            // Nejprve zjistíme počet záznamů
            var recordCount = await context.WeatherHistory
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .CountAsync();
            
            if (recordCount > MAX_RECORDS)
            {
                _logger.LogWarning("Příliš mnoho záznamů v zadaném intervalu ({ActualCount}). Omezeno na {MaxRecords} záznamů.", 
                    recordCount, MAX_RECORDS);
                
                // Vypočítáme interval, aby byl zhruba rovnoměrně rozložen
                var recordsPerDay = recordCount / (endDate - startDate).TotalDays;
                var skipInterval = recordCount / MAX_RECORDS;
                
                return await context.WeatherHistory
                    .Where(h => h.Date >= startDate && h.Date <= endDate)
                    .OrderBy(h => h.Date)
                    .Take(MAX_RECORDS)
                    .AsNoTracking() // Důležité pro snížení spotřeby paměti - Entity Framework nebude sledovat entity
                    .ToListAsync();
            }
            
            return await context.WeatherHistory
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .OrderBy(h => h.Date)
                .AsNoTracking() // Snížení spotřeby paměti
                .ToListAsync();
        }

        public async Task ImportCsvDataAsync(string csvContent)
        {
            if (string.IsNullOrWhiteSpace(csvContent))
            {
                throw new ArgumentException("CSV obsah je prázdný", nameof(csvContent));
            }

            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            // Přeskočíme hlavičkový řádek
            if (lines.Length <= 1)
            {
                _logger.LogWarning("CSV soubor neobsahuje žádná data kromě hlavičky");
                return;
            }

            // Použijeme konstanty pro optimalizaci
            const int BATCH_SIZE = 1000; // Zpracování po 1000 záznamech
            int totalImported = 0;
            int totalDuplicates = 0;
            int totalErrors = 0;

            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Získáme seznam všech existujících datumů jednou pro optimalizaci
            // Pro optimalizaci použijeme HashSet pro rychlé vyhledávání
            var allExistingDates = new HashSet<DateTime>(
                await context.WeatherHistory.Select(h => h.Date).ToListAsync()
            );

            // Zpracování po dávkách
            for (int batchStart = 1; batchStart < lines.Length; batchStart += BATCH_SIZE)
            {
                int batchEnd = Math.Min(batchStart + BATCH_SIZE, lines.Length);
                var currentBatch = new List<WeatherHistory>();
                
                // Parsování CSV řádků v aktuální dávce
                for (int i = batchStart; i < batchEnd; i++)
                {
                    try
                    {
                        var csvModel = CsvHistoryModel.ParseFromCsvLine(lines[i]);
                        
                        // Kontrola duplicit v paměti místo databázového dotazu
                        if (!allExistingDates.Contains(csvModel.Date))
                        {
                            var historyRecord = MapCsvModelToWeatherHistory(csvModel);
                            currentBatch.Add(historyRecord);
                            allExistingDates.Add(csvModel.Date); // Přidáme do množiny, abychom zabránili duplicitám v dalších dávkách
                        }
                        else
                        {
                            totalDuplicates++;
                        }
                    }
                    catch (Exception ex)
                    {
                        totalErrors++;
                        _logger.LogError(ex, "Chyba při zpracování CSV řádku {LineNumber}: {Line}", i, lines[i]);
                    }
                }

                // Uložíme aktuální dávku do databáze, pokud nějaké záznamy existují
                if (currentBatch.Any())
                {
                    await context.WeatherHistory.AddRangeAsync(currentBatch);
                    await context.SaveChangesAsync();
                    totalImported += currentBatch.Count;
                    
                    // Log průběhu po každé dávce
                    _logger.LogInformation("Zpracováno {Progress}% ({Current}/{Total}) záznamů", 
                        (int)((double)batchEnd / lines.Length * 100), 
                        batchEnd - 1, // -1 kvůli hlavičce
                        lines.Length - 1);
                }
            }

            // Shrnutí importu
            _logger.LogInformation("Import CSV dokončen: importováno {Imported}, přeskočeno {Duplicates} duplicit, chyby {Errors}", 
                totalImported, totalDuplicates, totalErrors);
        }

        public async Task FetchAndStoreEcowittDataAsync()
        {
            try
            {
                // Kontrola, zda je API povoleno v GlobalSettings
                bool isApiEnabled = _globalSettingsService.GetBool("Weather.IsEcowittApiEnabled", false);
                if (!isApiEnabled)
                {
                    _logger.LogInformation("Ecowitt API je zakázáno v nastaveních. Volání API přeskočeno.");
                    return;
                }
                
                // Zjistíme poslední datum záznamu
                var lastDate = await GetLastRecordDateAsync();
                
                // Pokud nemáme žádné záznamy v historii, neprovádíme volání API
                // Je potřeba nejprve importovat nějaká historická data pomocí CSV importu
                if (lastDate == null)
                {
                    _logger.LogWarning("V databázi nejsou žádná historická data. API Ecowitt nebude voláno. Nejprve importujte historická data pomocí CSV importu.");
                    return;
                }

                // Výchozí datum, pokud nemáme žádné záznamy
                var startDate = lastDate.Value.AddMinutes(1);
                var endDate = startDate.AddHours(12);

                // Vytvoříme URL pro API
                var apiUrl = BuildEcowittApiUrl(startDate, endDate);
                
                // Získáme data z API
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetFromJsonAsync<EcowittApiResponse>(apiUrl);

                if (response != null && response.Code == 0 && response.Data != null)
                {
                    var historyRecords = MapEcowittResponseToWeatherHistory(response.Data, startDate);
                    
                    // Uložíme nové záznamy do databáze
                    if (historyRecords.Any())
                    {
                        using var context = await _contextFactory.CreateDbContextAsync();
                        
                        // Kontrola a filtrování duplicitních záznamů
                        var existingDates = await context.WeatherHistory
                            .Where(h => historyRecords.Select(r => r.Date).Contains(h.Date))
                            .Select(h => h.Date)
                            .ToListAsync();

                        var newRecords = historyRecords.Where(r => !existingDates.Contains(r.Date)).ToList();
                        
                        if (newRecords.Any())
                        {
                            await context.WeatherHistory.AddRangeAsync(newRecords);
                            await context.SaveChangesAsync();
                            _logger.LogInformation("Získáno a uloženo {Count} nových záznamů z Ecowitt API", newRecords.Count);
                        }
                        else
                        {
                            _logger.LogInformation("Žádné nové záznamy k importu z Ecowitt API");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Neplatná odpověď z Ecowitt API: Kód {Code}, Zpráva: {Message}", 
                        response?.Code, response?.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při získávání dat z Ecowitt API");
            }
        }

        public async Task FetchAndStoreEcowittDataForPeriodAsync(DateTime startDate)
        {
            try
            {
                // Kontrola, zda je API povoleno v GlobalSettings
                bool isApiEnabled = _globalSettingsService.GetBool("Weather.IsEcowittApiEnabled", false);
                if (!isApiEnabled)
                {
                    _logger.LogInformation("Ecowitt API je zakázáno v nastaveních. Volání API přeskočeno.");
                    throw new InvalidOperationException("Ecowitt API je zakázáno v nastaveních. Pro stažení dat nejprve povolte API v nastavení.");
                }

                // Výchozí datum, pokud nemáme žádné záznamy
                var endDate = startDate.AddHours(12);

                // Vytvoříme URL pro API
                var apiUrl = BuildEcowittApiUrl(startDate, endDate);
                
                // Získáme data z API
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetFromJsonAsync<EcowittApiResponse>(apiUrl);

                if (response != null && response.Code == 0 && response.Data != null)
                {
                    var historyRecords = MapEcowittResponseToWeatherHistory(response.Data, startDate);
                    
                    // Uložíme nové záznamy do databáze
                    if (historyRecords.Any())
                    {
                        using var context = await _contextFactory.CreateDbContextAsync();
                        
                        // Kontrola a filtrování duplicitních záznamů
                        var existingDates = await context.WeatherHistory
                            .Where(h => historyRecords.Select(r => r.Date).Contains(h.Date))
                            .Select(h => h.Date)
                            .ToListAsync();

                        var newRecords = historyRecords.Where(r => !existingDates.Contains(r.Date)).ToList();
                        
                        if (newRecords.Any())
                        {
                            await context.WeatherHistory.AddRangeAsync(newRecords);
                            await context.SaveChangesAsync();
                            _logger.LogInformation("Získáno a uloženo {Count} nových záznamů z Ecowitt API", newRecords.Count);
                        }
                        else
                        {
                            _logger.LogInformation("Žádné nové záznamy k importu z Ecowitt API");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Neplatná odpověď z Ecowitt API: Kód {Code}, Zpráva: {Message}", 
                        response?.Code, response?.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při získávání dat z Ecowitt API");
            }
        }

        private string BuildEcowittApiUrl(DateTime startDate, DateTime endDate)
        {
            // Ujistíme se, že pracujeme s UTC datumy
            startDate = startDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) : startDate.ToUniversalTime();
            endDate = endDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) : endDate.ToUniversalTime();
            
            // Formátování datumů pro API - Ecowitt API očekává datumy ve formátu UTC
            var startDateStr = startDate.ToString("yyyy-MM-dd HH:mm:ss");
            var endDateStr = endDate.ToString("yyyy-MM-dd HH:mm:ss");
            
            _logger.LogInformation("Volání Ecowitt API od {StartDate} do {EndDate} (UTC)", startDateStr, endDateStr);

            // Použití konfiguračních hodnot místo konstant
            var urlBuilder = new StringBuilder(_ecowittSettings.ApiUrl);
            urlBuilder.Append($"?application_key={_ecowittSettings.ApplicationKey}");
            urlBuilder.Append($"&api_key={_ecowittSettings.ApiKey}");
            urlBuilder.Append($"&mac={_ecowittSettings.MacAddress}");
            urlBuilder.Append("&temp_unitid=1");  // Celsia
            urlBuilder.Append("&pressure_unitid=3");  // hPa
            urlBuilder.Append("&wind_speed_unitid=6");  // km/h
            urlBuilder.Append("&rainfall_unitid=12");  // mm
            urlBuilder.Append("&solar_irradiance_unitid=16");  // W/m²
            urlBuilder.Append("&capacity_unitid=24");  // %
            urlBuilder.Append($"&start_date={Uri.EscapeDataString(startDateStr)}");
            urlBuilder.Append($"&end_date={Uri.EscapeDataString(endDateStr)}");
            urlBuilder.Append("&cycle_type=5min");  // 5-minutové intervaly
            urlBuilder.Append("&call_back=outdoor,rainfall,indoor,solar_and_uvi,wind,pressure");

            return urlBuilder.ToString();
        }

        private List<WeatherHistory> MapEcowittResponseToWeatherHistory(EcowittData data, DateTime startDate)
        {
            var result = new List<WeatherHistory>();
            
            // Kontrola, zda data obsahují hodnoty
            if (data.Outdoor?.Temperature?.List == null || data.Outdoor.Temperature.List.Count == 0)
            {
                _logger.LogWarning("Ecowitt API nevrátilo žádné teplotní údaje");
                return result;
            }

            // Projdeme všechny epochové časové značky
            foreach (var timestamp in data.Outdoor.Temperature.List.Keys)
            {
                // Převedeme epochový čas (Unix timestamp) na DateTime
                if (!long.TryParse(timestamp, out long epochSeconds))
                {
                    _logger.LogWarning("Nelze převést epochový čas: {Time}", timestamp);
                    continue;
                }

                // Převedeme epochový čas na DateTime (epochový čas je v sekundách od 1.1.1970)
                var recordTime = DateTimeOffset.FromUnixTimeSeconds(epochSeconds).DateTime;
                
                // Explicitně označíme čas jako UTC
                recordTime = DateTime.SpecifyKind(recordTime, DateTimeKind.Utc);
                
                var historyRecord = new WeatherHistory
                {
                    Date = recordTime,
                    // Mapování hodnot podle dostupnosti
                    TemperatureOut = GetValueFromDict(data.Outdoor?.Temperature?.List, timestamp),
                    Chill = GetValueFromDict(data.Outdoor?.FeelsLike?.List, timestamp),
                    DewOut = GetValueFromDict(data.Outdoor?.DewPoint?.List, timestamp),
                    HumidityOut = GetValueFromDict(data.Outdoor?.Humidity?.List, timestamp),
                    RainRate = GetValueFromDict(data.Rainfall?.RainRate?.List, timestamp),
                    Rain = GetValueFromDict(data.Rainfall?.Daily?.List, timestamp),
                    TemperatureIn = GetValueFromDict(data.Indoor?.Temperature?.List, timestamp),
                    HumidityIn = GetValueFromDict(data.Indoor?.Humidity?.List, timestamp),
                    SolarRad = GetValueFromDict(data.SolarAndUvi?.Solar?.List, timestamp),
                    Uvi = GetIntValueFromDict(data.SolarAndUvi?.Uvi?.List, timestamp),
                    WindSpeedAvg = GetValueFromDict(data.Wind?.WindSpeed?.List, timestamp),
                    WindSpeedHi = GetValueFromDict(data.Wind?.WindGust?.List, timestamp),
                    WindDirection = GetValueFromDict(data.Wind?.WindDirection?.List, timestamp),
                    Bar = GetValueFromDict(data.Pressure?.Relative?.List, timestamp)
                };

                result.Add(historyRecord);
            }

            return result;
        }

        private float? GetValueFromDict(Dictionary<string, string> dict, string key)
        {
            if (dict == null || !dict.TryGetValue(key, out string value))
            {
                return null;
            }

            // Některé hodnoty mohou být "-" místo čísla
            if (value == "-")
            {
                return null;
            }

            if (float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }

            return null;
        }

        private int? GetIntValueFromDict(Dictionary<string, string> dict, string key)
        {
            if (dict == null || !dict.TryGetValue(key, out string value))
            {
                return null;
            }

            // Některé hodnoty mohou být "-" místo čísla
            if (value == "-")
            {
                return null;
            }

            if (int.TryParse(value, out int result))
            {
                return result;
            }

            return null;
        }

        private WeatherHistory MapCsvModelToWeatherHistory(CsvHistoryModel csvModel)
        {
            // Převod lokálního času na UTC
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(csvModel.Date, TimeZoneInfo.Local);
            
            _logger.LogInformation("Převod času z lokálního {LocalTime} na UTC {UtcTime}", 
                csvModel.Date.ToString("yyyy-MM-dd HH:mm:ss"), 
                utcDate.ToString("yyyy-MM-dd HH:mm:ss"));
            
            return new WeatherHistory
            {
                Date = utcDate, // Uložíme UTC čas
                TemperatureIn = csvModel.TemperatureIn,
                TemperatureOut = csvModel.TemperatureOut,
                Chill = csvModel.Chill,
                DewIn = csvModel.DewIn,
                DewOut = csvModel.DewOut,
                HeatIn = csvModel.HeatIn,
                Heat = csvModel.Heat,
                HumidityIn = csvModel.HumidityIn,
                HumidityOut = csvModel.HumidityOut,
                WindSpeedHi = csvModel.WindSpeedHi,
                WindSpeedAvg = csvModel.WindSpeedAvg,
                WindDirection = csvModel.WindDirection,
                Bar = csvModel.Bar,
                Rain = csvModel.Rain,
                RainRate = csvModel.RainRate,
                SolarRad = csvModel.SolarRad,
                Uvi = csvModel.Uvi
            };
        }
    }
} 