using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services.Weather
{
    public class TemperatureDataPoint
    {
        public DateTime Date { get; set; }
        public object DisplayTime { get; set; }
        public float? MinTemperature { get; set; }
        public float? AvgTemperature { get; set; }
        public float? MaxTemperature { get; set; }
    }

    public interface ITemperatureHistoryService
    {
        Task<List<TemperatureDataPoint>> GetTemperatureDataAsync(DateTime startDate, DateTime endDate, string aggregationType);
    }

    public class TemperatureHistoryService : ITemperatureHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<TemperatureHistoryService> _logger;
        private readonly TimeZoneInfo _localTimeZone;

        public TemperatureHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<TemperatureHistoryService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            // Získání lokální časové zóny
            _localTimeZone = TimeZoneInfo.Local;
            _logger.LogInformation("Použitá časová zóna: {TimeZone}", _localTimeZone.DisplayName);
        }

        public async Task<List<TemperatureDataPoint>> GetTemperatureDataAsync(DateTime startDate, DateTime endDate, string aggregationType)
        {
            _logger.LogInformation("GetTemperatureDataAsync - Načítání teplotních dat: {StartDate} až {EndDate}, typ agregace: {AggregationType}", 
                startDate, endDate, aggregationType);

            // Konverze vstupních parametrů na UTC pro dotazy do databáze
            DateTime startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            DateTime endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

            _logger.LogInformation("GetTemperatureDataAsync - Konvertované UTC časy: {StartDateUtc} až {EndDateUtc}", 
                startDateUtc, endDateUtc);

            using var context = await _contextFactory.CreateDbContextAsync();

            // Podle typu agregace zvolíme způsob, jakým budou data seskupena
            var result = aggregationType switch
            {
                "hourly" => await GetHourlyTemperatureDataAsync(context, startDateUtc, endDateUtc),
                "6hour" => await Get6HourTemperatureDataAsync(context, startDateUtc, endDateUtc),
                "daily" => await GetDailyTemperatureDataAsync(context, startDateUtc, endDateUtc),
                "weekly" => await GetWeeklyTemperatureDataAsync(context, startDateUtc, endDateUtc),
                "monthly" => await GetMonthlyTemperatureDataAsync(context, startDateUtc, endDateUtc),
                _ => await GetHourlyTemperatureDataAsync(context, startDateUtc, endDateUtc)
            };

            _logger.LogInformation("GetTemperatureDataAsync - Načteno {Count} záznamů", result.Count);
            return result;
        }

        private async Task<List<TemperatureDataPoint>> GetHourlyTemperatureDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetHourlyTemperatureDataAsync - Začátek načítání hodinových dat");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetHourlyTemperatureDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<TemperatureDataPoint>();
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
                    MinTemperature = g.Min(x => x.TemperatureOut),
                    AvgTemperature = g.Average(x => x.TemperatureOut),
                    MaxTemperature = g.Max(x => x.TemperatureOut)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day) 
                .ThenBy(d => d.Hour)
                .ToListAsync();

            // Poté transformujeme agregovaná data na TemperatureDataPoint a konvertujeme čas z UTC na lokální
            var result = aggregatedData.Select(d => 
            {
                // Vytvoříme UTC čas
                var utcDate = new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0, DateTimeKind.Utc);
                // Konvertujeme na lokální čas
                var localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, _localTimeZone);
                
                return new TemperatureDataPoint
                {
                    Date = localDate,
                    DisplayTime = localDate.ToString("HH:00"),
                    MinTemperature = d.MinTemperature,
                    AvgTemperature = d.AvgTemperature,
                    MaxTemperature = d.MaxTemperature
                };
            }).ToList();
            
            _logger.LogInformation("GetHourlyTemperatureDataAsync - Načteno {Count} hodinových záznamů", result.Count);
            return result;
        }

        private async Task<List<TemperatureDataPoint>> GetDailyTemperatureDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetDailyTemperatureDataAsync - Začátek načítání denních dat");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetDailyTemperatureDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<TemperatureDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy a seskupíme podle lokálního času
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.TemperatureOut
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
                    MinTemperature = g.Min(x => x.TemperatureOut),
                    AvgTemperature = g.Average(x => x.TemperatureOut),
                    MaxTemperature = g.Max(x => x.TemperatureOut)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day)
                .ToList();

            // Poté transformujeme agregovaná data na TemperatureDataPoint
            var result = groupedData.Select(d => new TemperatureDataPoint
            {
                Date = new DateTime(d.Year, d.Month, d.Day),
                DisplayTime = new DateTime(d.Year, d.Month, d.Day).ToString("dd.MM"),
                MinTemperature = d.MinTemperature,
                AvgTemperature = d.AvgTemperature,
                MaxTemperature = d.MaxTemperature
            }).ToList();
            
            _logger.LogInformation("GetDailyTemperatureDataAsync - Načteno {Count} denních záznamů", result.Count);
            return result;
        }

        private async Task<List<TemperatureDataPoint>> GetWeeklyTemperatureDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetWeeklyTemperatureDataAsync - Začátek načítání týdenních dat");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetWeeklyTemperatureDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<TemperatureDataPoint>();
            }
            
            // Konverze UTC času zpět na lokální pro správné týdenní zobrazení
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, _localTimeZone);
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, _localTimeZone);
            
            // Pro týdenní agregaci vytvoříme vlastní logiku pro seskupení po týdnech
            var result = new List<TemperatureDataPoint>();
            
            // Připravíme kalendářní týdny v lokálním čase
            var currentDate = startDateLocal.Date;
            while (currentDate <= endDateLocal)
            {
                var weekStart = currentDate;
                var weekEnd = weekStart.AddDays(6) > endDateLocal ? endDateLocal : weekStart.AddDays(6);

                _logger.LogInformation("GetWeeklyTemperatureDataAsync - Načítání dat pro týden: {WeekStart:dd.MM.yyyy} až {WeekEnd:dd.MM.yyyy}", 
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
                    var minTemp = weatherData.Min(x => x.TemperatureOut);
                    var avgTemp = weatherData.Average(x => x.TemperatureOut);
                    var maxTemp = weatherData.Max(x => x.TemperatureOut);
                    
                    result.Add(new TemperatureDataPoint
                    {
                        Date = weekStart,
                        DisplayTime = $"{weekStart:dd.MM} - {weekEnd:dd.MM}",
                        MinTemperature = minTemp,
                        AvgTemperature = avgTemp,
                        MaxTemperature = maxTemp
                    });
                    _logger.LogInformation("GetWeeklyTemperatureDataAsync - Data pro týden byla načtena");
                }
                else
                {
                    _logger.LogWarning("GetWeeklyTemperatureDataAsync - Žádná data pro týden: {WeekStart:dd.MM.yyyy} až {WeekEnd:dd.MM.yyyy}", 
                        weekStart, weekEnd);
                }
                
                // Posuneme se na další týden
                currentDate = currentDate.AddDays(7);
            }

            _logger.LogInformation("GetWeeklyTemperatureDataAsync - Načteno {Count} týdenních záznamů", result.Count);
            return result;
        }

        private async Task<List<TemperatureDataPoint>> GetMonthlyTemperatureDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetMonthlyTemperatureDataAsync - Začátek načítání měsíčních dat");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetMonthlyTemperatureDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<TemperatureDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy a seskupíme podle lokálního času
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.TemperatureOut
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
                    MinTemperature = g.Min(x => x.TemperatureOut),
                    AvgTemperature = g.Average(x => x.TemperatureOut),
                    MaxTemperature = g.Max(x => x.TemperatureOut)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();

            // Poté transformujeme agregovaná data na TemperatureDataPoint
            var result = groupedData.Select(d => new TemperatureDataPoint
            {
                Date = new DateTime(d.Year, d.Month, 1),
                DisplayTime = new DateTime(d.Year, d.Month, 1).ToString("MM.yyyy"),
                MinTemperature = d.MinTemperature,
                AvgTemperature = d.AvgTemperature,
                MaxTemperature = d.MaxTemperature
            }).ToList();
            
            _logger.LogInformation("GetMonthlyTemperatureDataAsync - Načteno {Count} měsíčních záznamů", result.Count);
            return result;
        }

        private async Task<List<TemperatureDataPoint>> Get6HourTemperatureDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("Get6HourTemperatureDataAsync - Začátek načítání 6hodinových dat");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("Get6HourTemperatureDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<TemperatureDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.TemperatureOut
            }).ToList();
            
            // Vypočítáme 6hodinové bloky - 0-5, 6-11, 12-17, 18-23
            var groupedData = localData
                .GroupBy(h => new 
                {
                    Year = h.LocalDate.Year,
                    Month = h.LocalDate.Month,
                    Day = h.LocalDate.Day,
                    SixHourBlock = h.LocalDate.Hour / 6
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    g.Key.SixHourBlock,
                    MinTemperature = g.Min(x => x.TemperatureOut),
                    AvgTemperature = g.Average(x => x.TemperatureOut),
                    MaxTemperature = g.Max(x => x.TemperatureOut)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day)
                .ThenBy(d => d.SixHourBlock)
                .ToList();

            // Transformujeme agregovaná data na TemperatureDataPoint
            var result = groupedData.Select(d =>
            {
                // Vypočítáme začátek 6hodinového bloku
                int startHour = d.SixHourBlock * 6;
                var blockDate = new DateTime(d.Year, d.Month, d.Day, startHour, 0, 0);
                
                return new TemperatureDataPoint
                {
                    Date = blockDate,
                    // Formát: DD.MM HH:00-HH:00
                    DisplayTime = $"{blockDate:dd.MM} {startHour:00}:00-{startHour+5:00}:59",
                    MinTemperature = d.MinTemperature,
                    AvgTemperature = d.AvgTemperature,
                    MaxTemperature = d.MaxTemperature
                };
            }).ToList();
            
            _logger.LogInformation("Get6HourTemperatureDataAsync - Načteno {Count} 6hodinových záznamů", result.Count);
            return result;
        }
    }
} 