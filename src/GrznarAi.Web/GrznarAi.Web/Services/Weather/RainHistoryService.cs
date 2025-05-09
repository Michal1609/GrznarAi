using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services.Weather
{
    public class RainDataPoint
    {
        public DateTime Date { get; set; }
        public object DisplayTime { get; set; }
        public float? Rain { get; set; }
    }

    public interface IRainHistoryService
    {
        Task<List<RainDataPoint>> GetRainDataAsync(DateTime startDate, DateTime endDate, string aggregationType);
    }

    public class RainHistoryService : IRainHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<RainHistoryService> _logger;
        private readonly TimeZoneInfo _localTimeZone;

        public RainHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<RainHistoryService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            // Získání lokální časové zóny
            _localTimeZone = TimeZoneInfo.Local;
            _logger.LogInformation("Použitá časová zóna: {TimeZone}", _localTimeZone.DisplayName);
        }

        public async Task<List<RainDataPoint>> GetRainDataAsync(DateTime startDate, DateTime endDate, string aggregationType)
        {
            _logger.LogInformation("GetRainDataAsync - Načítání dat o srážkách: {StartDate} až {EndDate}, typ agregace: {AggregationType}", 
                startDate, endDate, aggregationType);

            // Konverze vstupních parametrů na UTC pro dotazy do databáze
            DateTime startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            DateTime endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

            _logger.LogInformation("GetRainDataAsync - Konvertované UTC časy: {StartDateUtc} až {EndDateUtc}", 
                startDateUtc, endDateUtc);

            using var context = await _contextFactory.CreateDbContextAsync();

            // Podle typu agregace zvolíme způsob, jakým budou data seskupena
            var result = aggregationType switch
            {
                "hourly" => await GetHourlyRainDataAsync(context, startDateUtc, endDateUtc),
                "daily" => await GetDailyRainDataAsync(context, startDateUtc, endDateUtc),
                "weekly" => await GetWeeklyRainDataAsync(context, startDateUtc, endDateUtc),
                "monthly" => await GetMonthlyRainDataAsync(context, startDateUtc, endDateUtc),
                _ => await GetHourlyRainDataAsync(context, startDateUtc, endDateUtc)
            };

            _logger.LogInformation("GetRainDataAsync - Načteno {Count} záznamů", result.Count);
            return result;
        }

        private async Task<List<RainDataPoint>> GetHourlyRainDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetHourlyRainDataAsync - Začátek načítání hodinových dat o srážkách");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetHourlyRainDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<RainDataPoint>();
            }
            
            // Použijeme dvoustupňové zpracování - nejdřív získáme agregovaná data z UTC času
            var aggregatedData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .GroupBy(h => new
                {
                    Year = h.Date.Year,
                    Month = h.Date.Month,
                    Day = h.Date.Day,
                    Hour = h.Date.Hour
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    g.Key.Hour,
                    // Pro srážky použijeme maximum za danou hodinu
                    Rain = g.Max(x => x.Rain)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day) 
                .ThenBy(d => d.Hour)
                .ToListAsync();

            // Poté transformujeme agregovaná data na RainDataPoint a konvertujeme čas z UTC na lokální
            var result = aggregatedData.Select(d => 
            {
                // Vytvoříme UTC čas
                var utcDate = new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0, DateTimeKind.Utc);
                // Konvertujeme na lokální čas
                var localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, _localTimeZone);
                
                return new RainDataPoint
                {
                    Date = localDate,
                    DisplayTime = localDate.ToString("HH:00"),
                    Rain = d.Rain
                };
            }).ToList();
            
            _logger.LogInformation("GetHourlyRainDataAsync - Načteno {Count} hodinových záznamů", result.Count);
            return result;
        }

        private async Task<List<RainDataPoint>> GetDailyRainDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetDailyRainDataAsync - Začátek načítání denních dat o srážkách");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetDailyRainDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<RainDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy a seskupíme podle lokálního času
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.Rain
            }).ToList();
            
            // Seskupíme podle lokálních dnů
            var groupedData = localData
                .GroupBy(h => new 
                {
                    Year = h.LocalDate.Year,
                    Month = h.LocalDate.Month,
                    Day = h.LocalDate.Day
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    // Pro srážky použijeme maximum za daný den
                    Rain = g.Max(x => x.Rain)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day)
                .ToList();

            // Poté transformujeme agregovaná data na RainDataPoint
            var result = groupedData.Select(d => new RainDataPoint
            {
                Date = new DateTime(d.Year, d.Month, d.Day),
                DisplayTime = new DateTime(d.Year, d.Month, d.Day).ToString("dd.MM"),
                Rain = d.Rain
            }).ToList();
            
            _logger.LogInformation("GetDailyRainDataAsync - Načteno {Count} denních záznamů", result.Count);
            return result;
        }

        private async Task<List<RainDataPoint>> GetWeeklyRainDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetWeeklyRainDataAsync - Začátek načítání týdenních dat o srážkách");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetWeeklyRainDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<RainDataPoint>();
            }
            
            // Konverze UTC času zpět na lokální pro správné týdenní zobrazení
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, _localTimeZone);
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, _localTimeZone);
            
            // Pro týdenní agregaci vytvoříme vlastní logiku pro seskupení po týdnech
            var result = new List<RainDataPoint>();
            
            // Připravíme kalendářní týdny v lokálním čase
            var currentDate = startDateLocal.Date;
            while (currentDate <= endDateLocal)
            {
                var weekStart = currentDate;
                var weekEnd = weekStart.AddDays(6) > endDateLocal ? endDateLocal : weekStart.AddDays(6);

                _logger.LogInformation("GetWeeklyRainDataAsync - Načítání dat pro týden: {WeekStart:dd.MM.yyyy} až {WeekEnd:dd.MM.yyyy}", 
                    weekStart, weekEnd);
                
                // Konverze zpět na UTC pro dotaz do databáze
                var weekStartUtc = DateTime.SpecifyKind(weekStart, DateTimeKind.Local).ToUniversalTime();
                var weekEndUtc = DateTime.SpecifyKind(weekEnd.AddHours(23).AddMinutes(59).AddSeconds(59), DateTimeKind.Local).ToUniversalTime();

                // Získáme data z databáze
                var weatherData = await context.WeatherHistory
                    .Where(h => h.Date >= weekStartUtc && h.Date <= weekEndUtc)
                    .ToListAsync();
                    
                if (weatherData.Any())
                {
                    // Pro srážky použijeme maximum za týden
                    var rainMax = weatherData.Max(x => x.Rain) ?? 0;
                    
                    result.Add(new RainDataPoint
                    {
                        Date = weekStart,
                        DisplayTime = $"{weekStart:dd.MM} - {weekEnd:dd.MM}",
                        Rain = rainMax
                    });
                }
                
                currentDate = weekEnd.AddDays(1);
            }
            
            _logger.LogInformation("GetWeeklyRainDataAsync - Načteno {Count} týdenních záznamů", result.Count);
            return result;
        }

        private async Task<List<RainDataPoint>> GetMonthlyRainDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetMonthlyRainDataAsync - Začátek načítání měsíčních dat o srážkách");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetMonthlyRainDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<RainDataPoint>();
            }
            
            // Konverze UTC času zpět na lokální pro správné měsíční zobrazení
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, _localTimeZone);
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, _localTimeZone);
            
            // Získáme data z databáze a konvertujeme je na lokální čas
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.Rain
            }).ToList();
            
            // Seskupíme podle měsíců
            var groupedByMonth = localData
                .GroupBy(h => new 
                {
                    Year = h.LocalDate.Year,
                    Month = h.LocalDate.Month
                })
                .Select(g => new 
                {
                    g.Key.Year,
                    g.Key.Month,
                    // Pro srážky použijeme maximum za daný měsíc
                    Rain = g.Max(x => x.Rain)
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();
                
            // Transformujeme na RainDataPoint
            var result = groupedByMonth.Select(m => new RainDataPoint
            {
                Date = new DateTime(m.Year, m.Month, 1),
                DisplayTime = new DateTime(m.Year, m.Month, 1).ToString("MMMM yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("cs-CZ")),
                Rain = m.Rain
            }).ToList();
            
            _logger.LogInformation("GetMonthlyRainDataAsync - Načteno {Count} měsíčních záznamů", result.Count);
            return result;
        }
    }
} 