using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services.Weather
{
    public class HumidityDataPoint
    {
        public DateTime Date { get; set; }
        public object DisplayTime { get; set; }
        public float? MinHumidity { get; set; }
        public float? AvgHumidity { get; set; }
        public float? MaxHumidity { get; set; }
    }

    public interface IHumidityHistoryService
    {
        Task<List<HumidityDataPoint>> GetHumidityDataAsync(DateTime startDate, DateTime endDate, string aggregationType);
    }

    public class HumidityHistoryService : IHumidityHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<HumidityHistoryService> _logger;
        private readonly TimeZoneInfo _localTimeZone;

        public HumidityHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<HumidityHistoryService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            // Získání lokální časové zóny
            _localTimeZone = TimeZoneInfo.Local;
            _logger.LogInformation("Použitá časová zóna: {TimeZone}", _localTimeZone.DisplayName);
        }

        public async Task<List<HumidityDataPoint>> GetHumidityDataAsync(DateTime startDate, DateTime endDate, string aggregationType)
        {
            _logger.LogInformation("GetHumidityDataAsync - Načítání dat vlhkosti: {StartDate} až {EndDate}, typ agregace: {AggregationType}", 
                startDate, endDate, aggregationType);

            // Konverze vstupních parametrů na UTC pro dotazy do databáze
            DateTime startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            DateTime endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

            _logger.LogInformation("GetHumidityDataAsync - Konvertované UTC časy: {StartDateUtc} až {EndDateUtc}", 
                startDateUtc, endDateUtc);

            using var context = await _contextFactory.CreateDbContextAsync();

            // Podle typu agregace zvolíme způsob, jakým budou data seskupena
            var result = aggregationType switch
            {
                "hourly" => await GetHourlyHumidityDataAsync(context, startDateUtc, endDateUtc),
                "daily" => await GetDailyHumidityDataAsync(context, startDateUtc, endDateUtc),
                "weekly" => await GetWeeklyHumidityDataAsync(context, startDateUtc, endDateUtc),
                "monthly" => await GetMonthlyHumidityDataAsync(context, startDateUtc, endDateUtc),
                _ => await GetHourlyHumidityDataAsync(context, startDateUtc, endDateUtc)
            };

            _logger.LogInformation("GetHumidityDataAsync - Načteno {Count} záznamů", result.Count);
            return result;
        }

        private async Task<List<HumidityDataPoint>> GetHourlyHumidityDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetHourlyHumidityDataAsync - Začátek načítání hodinových dat vlhkosti");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetHourlyHumidityDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<HumidityDataPoint>();
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
                    MinHumidity = g.Min(x => x.HumidityOut),
                    AvgHumidity = g.Average(x => x.HumidityOut),
                    MaxHumidity = g.Max(x => x.HumidityOut)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day) 
                .ThenBy(d => d.Hour)
                .ToListAsync();

            // Poté transformujeme agregovaná data na HumidityDataPoint a konvertujeme čas z UTC na lokální
            var result = aggregatedData.Select(d => 
            {
                // Vytvoříme UTC čas
                var utcDate = new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0, DateTimeKind.Utc);
                // Konvertujeme na lokální čas
                var localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, _localTimeZone);
                
                return new HumidityDataPoint
                {
                    Date = localDate,
                    DisplayTime = localDate.ToString("HH:00"),
                    MinHumidity = d.MinHumidity,
                    AvgHumidity = d.AvgHumidity,
                    MaxHumidity = d.MaxHumidity
                };
            }).ToList();
            
            _logger.LogInformation("GetHourlyHumidityDataAsync - Načteno {Count} hodinových záznamů vlhkosti", result.Count);
            return result;
        }

        private async Task<List<HumidityDataPoint>> GetDailyHumidityDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetDailyHumidityDataAsync - Začátek načítání denních dat vlhkosti");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetDailyHumidityDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<HumidityDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy a seskupíme podle lokálního času
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.HumidityOut
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
                    MinHumidity = g.Min(x => x.HumidityOut),
                    AvgHumidity = g.Average(x => x.HumidityOut),
                    MaxHumidity = g.Max(x => x.HumidityOut)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day)
                .ToList();

            // Poté transformujeme agregovaná data na HumidityDataPoint
            var result = groupedData.Select(d => new HumidityDataPoint
            {
                Date = new DateTime(d.Year, d.Month, d.Day),
                DisplayTime = new DateTime(d.Year, d.Month, d.Day).ToString("dd.MM"),
                MinHumidity = d.MinHumidity,
                AvgHumidity = d.AvgHumidity,
                MaxHumidity = d.MaxHumidity
            }).ToList();
            
            _logger.LogInformation("GetDailyHumidityDataAsync - Načteno {Count} denních záznamů vlhkosti", result.Count);
            return result;
        }

        private async Task<List<HumidityDataPoint>> GetWeeklyHumidityDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetWeeklyHumidityDataAsync - Začátek načítání týdenních dat vlhkosti");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetWeeklyHumidityDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<HumidityDataPoint>();
            }
            
            // Konverze UTC času zpět na lokální pro správné týdenní zobrazení
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, _localTimeZone);
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, _localTimeZone);
            
            // Pro týdenní agregaci vytvoříme vlastní logiku pro seskupení po týdnech
            var result = new List<HumidityDataPoint>();
            
            // Připravíme kalendářní týdny v lokálním čase
            var currentDate = startDateLocal.Date;
            while (currentDate <= endDateLocal)
            {
                var weekStart = currentDate;
                var weekEnd = weekStart.AddDays(6) > endDateLocal ? endDateLocal : weekStart.AddDays(6);

                _logger.LogInformation("GetWeeklyHumidityDataAsync - Načítání dat pro týden: {WeekStart:dd.MM.yyyy} až {WeekEnd:dd.MM.yyyy}", 
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
                    var minHumidity = weatherData.Min(x => x.HumidityOut);
                    var avgHumidity = weatherData.Average(x => x.HumidityOut);
                    var maxHumidity = weatherData.Max(x => x.HumidityOut);
                    
                    result.Add(new HumidityDataPoint
                    {
                        Date = weekStart,
                        DisplayTime = $"{weekStart:dd.MM} - {weekEnd:dd.MM}",
                        MinHumidity = minHumidity,
                        AvgHumidity = avgHumidity,
                        MaxHumidity = maxHumidity
                    });
                    _logger.LogInformation("GetWeeklyHumidityDataAsync - Data pro týden byla načtena");
                }
                else
                {
                    _logger.LogWarning("GetWeeklyHumidityDataAsync - Žádná data pro týden: {WeekStart:dd.MM.yyyy} až {WeekEnd:dd.MM.yyyy}", 
                        weekStart, weekEnd);
                }
                
                // Posuneme se na další týden
                currentDate = currentDate.AddDays(7);
            }

            _logger.LogInformation("GetWeeklyHumidityDataAsync - Načteno {Count} týdenních záznamů vlhkosti", result.Count);
            return result;
        }

        private async Task<List<HumidityDataPoint>> GetMonthlyHumidityDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetMonthlyHumidityDataAsync - Začátek načítání měsíčních dat vlhkosti");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetMonthlyHumidityDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<HumidityDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy a seskupíme podle lokálního času
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.HumidityOut
            }).ToList();
            
            // Seskupíme podle lokálních měsíců
            var groupedData = localData
                .GroupBy(h => new 
                {
                    Year = h.LocalDate.Year,
                    Month = h.LocalDate.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    MinHumidity = g.Min(x => x.HumidityOut),
                    AvgHumidity = g.Average(x => x.HumidityOut),
                    MaxHumidity = g.Max(x => x.HumidityOut)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();

            // Poté transformujeme agregovaná data na HumidityDataPoint
            var result = groupedData.Select(d => new HumidityDataPoint
            {
                Date = new DateTime(d.Year, d.Month, 1),
                DisplayTime = new DateTime(d.Year, d.Month, 1).ToString("MM.yyyy"),
                MinHumidity = d.MinHumidity,
                AvgHumidity = d.AvgHumidity,
                MaxHumidity = d.MaxHumidity
            }).ToList();
            
            _logger.LogInformation("GetMonthlyHumidityDataAsync - Načteno {Count} měsíčních záznamů vlhkosti", result.Count);
            return result;
        }
    }
} 