using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace GrznarAi.Web.Services.Weather
{
    public class UVIndexDataPoint
    {
        public DateTime Date { get; set; }
        public object DisplayTime { get; set; }
        public float? AvgUVIndex { get; set; }
        public float? MaxUVIndex { get; set; }
    }

    public interface IUVIndexHistoryService
    {
        Task<List<UVIndexDataPoint>> GetUVIndexDataAsync(DateTime startDate, DateTime endDate, string aggregationType);
    }

    public class UVIndexHistoryService : IUVIndexHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<UVIndexHistoryService> _logger;
        private readonly TimeZoneInfo _localTimeZone;

        public UVIndexHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<UVIndexHistoryService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            // Získání lokální časové zóny
            _localTimeZone = TimeZoneInfo.Local;
            _logger.LogInformation("Použitá časová zóna: {TimeZone}", _localTimeZone.DisplayName);
        }

        public async Task<List<UVIndexDataPoint>> GetUVIndexDataAsync(DateTime startDate, DateTime endDate, string aggregationType)
        {
            _logger.LogInformation("GetUVIndexDataAsync - Načítání dat UV indexu: {StartDate} až {EndDate}, typ agregace: {AggregationType}", 
                startDate, endDate, aggregationType);

            // Konverze vstupních parametrů na UTC pro dotazy do databáze
            DateTime startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            DateTime endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.WeatherHistory.AsNoTracking()
                    .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                    .OrderBy(h => h.Date);

                // Načtení dat z databáze
                var weatherData = await query.ToListAsync();

                if (weatherData == null || !weatherData.Any())
                {
                    _logger.LogWarning("Nenalezena žádná data UV indexu pro období {StartDate} až {EndDate}", startDate, endDate);
                    return new List<UVIndexDataPoint>();
                }

                // Převedeme data z UTC zpět do lokálního času pro zobrazení
                foreach (var item in weatherData)
                {
                    item.Date = TimeZoneInfo.ConvertTimeFromUtc(item.Date, _localTimeZone);
                }

                // Podle typu agregace zvolíme způsob, jakým budou data seskupena
                var result = aggregationType switch
                {
                    "hourly" => await GetHourlyUVIndexDataAsync(context, startDateUtc, endDateUtc),
                    "6hour" => await Get6HourUVIndexDataAsync(context, startDateUtc, endDateUtc),
                    "daily" => await GetDailyUVIndexDataAsync(context, startDateUtc, endDateUtc),
                    "weekly" => await GetWeeklyUVIndexDataAsync(context, startDateUtc, endDateUtc),
                    "monthly" => await GetMonthlyUVIndexDataAsync(context, startDateUtc, endDateUtc),
                    _ => await GetHourlyUVIndexDataAsync(context, startDateUtc, endDateUtc)
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání dat UV indexu");
                return new List<UVIndexDataPoint>();
            }
        }

        private List<UVIndexDataPoint> AggregateByHour(List<WeatherHistory> weatherData)
        {
            var result = weatherData
                .GroupBy(h => new { Hour = new DateTime(h.Date.Year, h.Date.Month, h.Date.Day, h.Date.Hour, 0, 0) })
                .Select(g => new UVIndexDataPoint
                {
                    Date = g.Key.Hour,
                    DisplayTime = g.Key.Hour.ToString("HH:00"),
                    AvgUVIndex = g.Where(h => h.Uvi.HasValue).Average(h => (float)h.Uvi),
                    MaxUVIndex = g.Where(h => h.Uvi.HasValue).Max(h => (float)h.Uvi)
                })
                .OrderBy(h => h.Date)
                .ToList();

            return result;
        }

        private List<UVIndexDataPoint> AggregateByDay(List<WeatherHistory> weatherData)
        {
            var result = weatherData
                .GroupBy(h => new { Day = new DateTime(h.Date.Year, h.Date.Month, h.Date.Day) })
                .Select(g => new UVIndexDataPoint
                {
                    Date = g.Key.Day,
                    DisplayTime = g.Key.Day.ToString("dd.MM."),
                    AvgUVIndex = g.Where(h => h.Uvi.HasValue).Average(h => (float)h.Uvi),
                    MaxUVIndex = g.Where(h => h.Uvi.HasValue).Max(h => (float)h.Uvi)
                })
                .OrderBy(h => h.Date)
                .ToList();

            return result;
        }

        private async Task<List<UVIndexDataPoint>> AggregateMonthlyUVIndexDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc && w.Uvi.HasValue)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                UVIndex = (float)h.Uvi
            }).ToList();

            // Seskupíme data v paměti podle lokálního času
            var result = localData
                .GroupBy(w => new
                {
                    Year = w.LocalDate.Year,
                    Month = w.LocalDate.Month
                })
                .Select(g => new UVIndexDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    AvgUVIndex = g.Average(w => w.UVIndex),
                    MaxUVIndex = g.Max(w => w.UVIndex)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<UVIndexDataPoint>> GetHourlyUVIndexDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetHourlyUVIndexDataAsync - Začátek načítání hodinových dat UV indexu");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetHourlyUVIndexDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<UVIndexDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                UVIndex = h.Uvi ?? 0 // Oprava: použití Uvi místo UVIndex a zajištění nenulové hodnoty
            }).ToList();
            
            // Vypočítáme hodinové bloky - 0-1, 1-2, ..., 23-24
            var groupedData = localData
                .GroupBy(h => new 
                {
                    Year = h.LocalDate.Year,
                    Month = h.LocalDate.Month,
                    Day = h.LocalDate.Day,
                    Hour = h.LocalDate.Hour
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    g.Key.Hour,
                    AvgUVIndex = g.Average(x => x.UVIndex),
                    MaxUVIndex = g.Max(x => x.UVIndex)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day)
                .ThenBy(d => d.Hour)
                .ToList();

            // Transformujeme agregovaná data na UVIndexDataPoint
            var result = groupedData.Select(d =>
            {
                // Vypočítáme začátek hodinového bloku
                int startHour = d.Hour;
                var blockDate = new DateTime(d.Year, d.Month, d.Day, startHour, 0, 0);
                
                return new UVIndexDataPoint
                {
                    Date = blockDate,
                    // Formát: DD.MM HH:00-HH:00
                    DisplayTime = $"{blockDate:dd.MM} {startHour:00}:00-{startHour+1:00}:59",
                    AvgUVIndex = (float)d.AvgUVIndex, // Explicitní přetypování na float
                    MaxUVIndex = (float)d.MaxUVIndex  // Explicitní přetypování na float
                };
            }).ToList();
            
            _logger.LogInformation("GetHourlyUVIndexDataAsync - Načteno {Count} hodinových záznamů UV indexu", result.Count);
            return result;
        }

        private async Task<List<UVIndexDataPoint>> Get6HourUVIndexDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("Get6HourUVIndexDataAsync - Začátek načítání 6hodinových dat UV indexu");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("Get6HourUVIndexDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<UVIndexDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                UVIndex = h.Uvi ?? 0 // Oprava: použití Uvi místo UVIndex a zajištění nenulové hodnoty
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
                    AvgUVIndex = g.Average(x => x.UVIndex),
                    MaxUVIndex = g.Max(x => x.UVIndex)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day)
                .ThenBy(d => d.SixHourBlock)
                .ToList();

            // Transformujeme agregovaná data na UVIndexDataPoint
            var result = groupedData.Select(d =>
            {
                // Vypočítáme začátek 6hodinového bloku
                int startHour = d.SixHourBlock * 6;
                var blockDate = new DateTime(d.Year, d.Month, d.Day, startHour, 0, 0);
                
                return new UVIndexDataPoint
                {
                    Date = blockDate,
                    // Formát: DD.MM HH:00-HH:00
                    DisplayTime = $"{blockDate:dd.MM} {startHour:00}:00-{startHour+5:00}:59",
                    AvgUVIndex = (float)d.AvgUVIndex, // Explicitní přetypování na float
                    MaxUVIndex = (float)d.MaxUVIndex  // Explicitní přetypování na float
                };
            }).ToList();
            
            _logger.LogInformation("Get6HourUVIndexDataAsync - Načteno {Count} 6hodinových záznamů UV indexu", result.Count);
            return result;
        }

        private async Task<List<UVIndexDataPoint>> GetDailyUVIndexDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetDailyUVIndexDataAsync - Začátek načítání denních dat UV indexu");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetDailyUVIndexDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<UVIndexDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                UVIndex = h.Uvi ?? 0 // Oprava: použití Uvi místo UVIndex a zajištění nenulové hodnoty
            }).ToList();
            
            // Vypočítáme denní bloky - 1-7
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
                    AvgUVIndex = g.Average(x => x.UVIndex),
                    MaxUVIndex = g.Max(x => x.UVIndex)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ThenBy(d => d.Day)
                .ToList();

            // Transformujeme agregovaná data na UVIndexDataPoint
            var result = groupedData.Select(d =>
            {
                var blockDate = new DateTime(d.Year, d.Month, d.Day);
                
                return new UVIndexDataPoint
                {
                    Date = blockDate,
                    // Formát: DD.MM
                    DisplayTime = blockDate.ToString("dd.MM"),
                    AvgUVIndex = (float)d.AvgUVIndex, // Explicitní přetypování na float
                    MaxUVIndex = (float)d.MaxUVIndex  // Explicitní přetypování na float
                };
            }).ToList();
            
            _logger.LogInformation("GetDailyUVIndexDataAsync - Načteno {Count} denních záznamů UV indexu", result.Count);
            return result;
        }

        private async Task<List<UVIndexDataPoint>> GetWeeklyUVIndexDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetWeeklyUVIndexDataAsync - Začátek načítání týdenních dat UV indexu");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetWeeklyUVIndexDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<UVIndexDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
            
            // Konverze UTC času zpět na lokální pro správné týdenní zobrazení
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, _localTimeZone);
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, _localTimeZone);
            
            // Pro týdenní agregaci vytvoříme vlastní logiku pro seskupení po týdnech
            var result = new List<UVIndexDataPoint>();
            
            // Připravíme kalendářní týdny v lokálním čase
            var currentDate = startDateLocal.Date;
            while (currentDate <= endDateLocal)
            {
                var weekStart = currentDate;
                var weekEnd = weekStart.AddDays(6) > endDateLocal ? endDateLocal : weekStart.AddDays(6);

                _logger.LogInformation("GetWeeklyUVIndexDataAsync - Načítání dat pro týden: {WeekStart} až {WeekEnd}", 
                    weekStart.ToString("dd.MM.yyyy"), weekEnd.ToString("dd.MM.yyyy"));
                
                // Konverze zpět na UTC pro dotaz do databáze
                var weekStartUtc = DateTime.SpecifyKind(weekStart, DateTimeKind.Local).ToUniversalTime();
                var weekEndUtc = DateTime.SpecifyKind(weekEnd.AddHours(23).AddMinutes(59).AddSeconds(59), DateTimeKind.Local).ToUniversalTime();

                // Filtrujeme data pro tento týden
                var weekData = weatherData
                    .Where(h => h.Date >= weekStartUtc && h.Date <= weekEndUtc)
                    .ToList();
                    
                if (weekData.Any())
                {
                    result.Add(new UVIndexDataPoint
                    {
                        Date = weekStart,
                        DisplayTime = $"{weekStart:dd.MM} - {weekEnd:dd.MM}",
                        AvgUVIndex = (float)weekData.Average(x => x.Uvi ?? 0),
                        MaxUVIndex = (float)weekData.Max(x => x.Uvi ?? 0)
                    });
                }
                
                // Posuneme se na další týden
                currentDate = currentDate.AddDays(7);
            }
            
            _logger.LogInformation("GetWeeklyUVIndexDataAsync - Načteno {Count} týdenních záznamů UV indexu", result.Count);
            return result;
        }

        private async Task<List<UVIndexDataPoint>> GetMonthlyUVIndexDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("GetMonthlyUVIndexDataAsync - Začátek načítání měsíčních dat UV indexu");
            
            // Kontrola, zda existují nějaká data pro dané období
            var dataExists = await context.WeatherHistory
                .AnyAsync(h => h.Date >= startDateUtc && h.Date <= endDateUtc);
                
            if (!dataExists)
            {
                _logger.LogWarning("GetMonthlyUVIndexDataAsync - Žádná data v databázi pro dané období: {StartDate} až {EndDate}", 
                    startDateUtc, endDateUtc);
                return new List<UVIndexDataPoint>();
            }
            
            // Získáme data z databáze
            var weatherData = await context.WeatherHistory
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .ToListAsync();
                
            // Nyní konvertujeme UTC časy na lokální časy
            var localData = weatherData.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                UVIndex = h.Uvi ?? 0 // Oprava: použití Uvi místo UVIndex a zajištění nenulové hodnoty
            }).ToList();
            
            // Vypočítáme měsíční bloky - 1-12
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
                    AvgUVIndex = g.Average(x => x.UVIndex),
                    MaxUVIndex = g.Max(x => x.UVIndex)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();

            // Transformujeme agregovaná data na UVIndexDataPoint
            var result = groupedData.Select(d =>
            {
                var blockDate = new DateTime(d.Year, d.Month, 1);
                
                return new UVIndexDataPoint
                {
                    Date = blockDate,
                    // Formát: MM.YYYY
                    DisplayTime = blockDate.ToString("MM.yyyy"),
                    AvgUVIndex = (float)d.AvgUVIndex, // Explicitní přetypování na float
                    MaxUVIndex = (float)d.MaxUVIndex  // Explicitní přetypování na float
                };
            }).ToList();
            
            _logger.LogInformation("GetMonthlyUVIndexDataAsync - Načteno {Count} měsíčních záznamů UV indexu", result.Count);
            return result;
        }
    }
} 