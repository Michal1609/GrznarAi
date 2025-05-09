using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services.Weather
{
    public class SolarRadiationDataPoint
    {
        public DateTime Date { get; set; }
        public object DisplayTime { get; set; }
        public float? MinSolarRadiation { get; set; }
        public float? AvgSolarRadiation { get; set; }
        public float? MaxSolarRadiation { get; set; }
        public float? SunshineHours { get; set; }
    }

    public interface ISolarRadiationHistoryService
    {
        Task<List<SolarRadiationDataPoint>> GetSolarRadiationDataAsync(DateTime startDate, DateTime endDate, string aggregationType);
    }

    public class SolarRadiationHistoryService : ISolarRadiationHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<SolarRadiationHistoryService> _logger;
        private readonly TimeZoneInfo _localTimeZone;
        private const float SUNSHINE_THRESHOLD = 120.0f; // 120 W/m² práh pro rozpoznání slunečního svitu

        public SolarRadiationHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<SolarRadiationHistoryService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            // Získání lokální časové zóny
            _localTimeZone = TimeZoneInfo.Local;
            _logger.LogInformation("Použitá časová zóna: {TimeZone}", _localTimeZone.DisplayName);
        }

        public async Task<List<SolarRadiationDataPoint>> GetSolarRadiationDataAsync(DateTime startDate, DateTime endDate, string aggregationType)
        {
            _logger.LogInformation("GetSolarRadiationDataAsync - Načítání dat slunečního záření: {StartDate} až {EndDate}, typ agregace: {AggregationType}", 
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
                    _logger.LogWarning("Nenalezena žádná data slunečního záření pro období {StartDate} až {EndDate}", startDate, endDate);
                    return new List<SolarRadiationDataPoint>();
                }

                // Převedeme data z UTC zpět do lokálního času pro zobrazení
                foreach (var item in weatherData)
                {
                    item.Date = TimeZoneInfo.ConvertTimeFromUtc(item.Date, _localTimeZone);
                }

                // Podle typu agregace zvolíme způsob, jakým budou data seskupena
                List<SolarRadiationDataPoint> result;

                switch (aggregationType.ToLower())
                {
                    case "hourly":
                        result = await AggregateByHourAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "6hour":
                        result = await AggregateBy6HourAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "daily":
                        result = await AggregateByDayAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "weekly":
                        result = await AggregateByWeekAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "monthly":
                        result = await AggregateByMonthAsync(context, startDateUtc, endDateUtc);
                        break;
                    default:
                        _logger.LogWarning("Neznámý typ agregace: {AggregationType}, použije se hodinová agregace", aggregationType);
                        result = await AggregateByHourAsync(context, startDateUtc, endDateUtc);
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání dat slunečního záření: {Message}", ex.Message);
                return new List<SolarRadiationDataPoint>();
            }
        }

        private async Task<List<SolarRadiationDataPoint>> AggregateByHourAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            var query = context.WeatherHistory.AsNoTracking()
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .OrderBy(h => h.Date);

            var weatherData = await query.ToListAsync();

            if (weatherData == null || !weatherData.Any())
            {
                _logger.LogWarning("Nenalezena žádná data slunečního záření pro období {StartDate} až {EndDate}", startDateUtc, endDateUtc);
                return new List<SolarRadiationDataPoint>();
            }

            var result = weatherData
                .GroupBy(h => new { Hour = new DateTime(h.Date.Year, h.Date.Month, h.Date.Day, h.Date.Hour, 0, 0) })
                .Select(g => new SolarRadiationDataPoint
                {
                    Date = g.Key.Hour,
                    DisplayTime = g.Key.Hour.ToString("HH:00"),
                    MinSolarRadiation = g.Where(h => h.SolarRad.HasValue).Min(h => h.SolarRad),
                    AvgSolarRadiation = g.Where(h => h.SolarRad.HasValue).Average(h => h.SolarRad),
                    MaxSolarRadiation = g.Where(h => h.SolarRad.HasValue).Max(h => h.SolarRad),
                    SunshineHours = g.Count() > 0 
                        ? (float)g.Count(h => h.SolarRad.HasValue && h.SolarRad.Value >= SUNSHINE_THRESHOLD) / g.Count() 
                        : 0
                })
                .OrderBy(h => h.Date)
                .ToList();

            return result;
        }

        private async Task<List<SolarRadiationDataPoint>> AggregateBy6HourAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc)
                .ToListAsync();

            // Konvertujeme UTC časy na lokální
            var localData = data.Select(w => new
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(w.Date, DateTimeKind.Utc), _localTimeZone),
                w.SolarRad
            }).ToList();

            // Vypočítáme 6hodinové bloky - 0-5, 6-11, 12-17, 18-23
            var result = localData
                .GroupBy(h => new
                {
                    Year = h.LocalDate.Year,
                    Month = h.LocalDate.Month,
                    Day = h.LocalDate.Day,
                    SixHourBlock = h.LocalDate.Hour / 6
                })
                .Select(g =>
                {
                    // Vypočítáme začátek 6hodinového bloku
                    int startHour = g.Key.SixHourBlock * 6;
                    var blockDate = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, startHour, 0, 0);

                    // Hodiny slunečního svitu počítáme jako poměr měření nad prahem / počet měření * délka agregačního období v hodinách
                    float sunshineHours = g.Count() > 0
                        ? (float)g.Count(h => h.SolarRad.HasValue && h.SolarRad.Value >= SUNSHINE_THRESHOLD) / g.Count() * 6
                        : 0;

                    return new SolarRadiationDataPoint
                    {
                        Date = blockDate,
                        DisplayTime = $"{blockDate:dd.MM} {startHour:00}:00-{startHour+5:00}:59",
                        MinSolarRadiation = g.Where(h => h.SolarRad.HasValue).Min(h => h.SolarRad),
                        AvgSolarRadiation = g.Where(h => h.SolarRad.HasValue).Average(h => h.SolarRad),
                        MaxSolarRadiation = g.Where(h => h.SolarRad.HasValue).Max(h => h.SolarRad),
                        SunshineHours = sunshineHours
                    };
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<SolarRadiationDataPoint>> AggregateByDayAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            var query = context.WeatherHistory.AsNoTracking()
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .OrderBy(h => h.Date);

            var weatherData = await query.ToListAsync();

            if (weatherData == null || !weatherData.Any())
            {
                _logger.LogWarning("Nenalezena žádná data slunečního záření pro období {StartDate} až {EndDate}", startDateUtc, endDateUtc);
                return new List<SolarRadiationDataPoint>();
            }

            var result = weatherData
                .GroupBy(h => new { Day = new DateTime(h.Date.Year, h.Date.Month, h.Date.Day) })
                .Select(g => new SolarRadiationDataPoint
                {
                    Date = g.Key.Day,
                    DisplayTime = g.Key.Day.ToString("dd.MM."),
                    MinSolarRadiation = g.Where(h => h.SolarRad.HasValue).Min(h => h.SolarRad),
                    AvgSolarRadiation = g.Where(h => h.SolarRad.HasValue).Average(h => h.SolarRad),
                    MaxSolarRadiation = g.Where(h => h.SolarRad.HasValue).Max(h => h.SolarRad),
                    SunshineHours = g.Count() > 0 
                        ? (float)g.Count(h => h.SolarRad.HasValue && h.SolarRad.Value >= SUNSHINE_THRESHOLD) / g.Count() * 24 
                        : 0
                })
                .OrderBy(h => h.Date)
                .ToList();

            return result;
        }

        private async Task<List<SolarRadiationDataPoint>> AggregateByWeekAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc)
                .ToListAsync();

            // Konverze UTC času zpět na lokální pro správné týdenní zobrazení
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, _localTimeZone);
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, _localTimeZone);
            
            // Pro týdenní agregaci vytvoříme vlastní logiku pro seskupení po týdnech
            var result = new List<SolarRadiationDataPoint>();
            
            // Připravíme kalendářní týdny v lokálním čase
            var currentDate = startDateLocal.Date;
            while (currentDate <= endDateLocal)
            {
                var weekStart = currentDate;
                var weekEnd = weekStart.AddDays(6) > endDateLocal ? endDateLocal : weekStart.AddDays(6);

                _logger.LogInformation("AggregateByWeekAsync - Načítání dat pro týden: {WeekStart:dd.MM.yyyy} až {WeekEnd:dd.MM.yyyy}", 
                    weekStart, weekEnd);
                
                // Konverze zpět na UTC pro dotaz do databáze
                var weekStartUtc = DateTime.SpecifyKind(weekStart, DateTimeKind.Local).ToUniversalTime();
                var weekEndUtc = DateTime.SpecifyKind(weekEnd.AddHours(23).AddMinutes(59).AddSeconds(59), DateTimeKind.Local).ToUniversalTime();

                // Filtrujeme data pro tento týden
                var weekData = data.Where(w => w.Date >= weekStartUtc && w.Date <= weekEndUtc).ToList();
                    
                if (weekData.Any())
                {
                    // Počítáme hodiny slunečního svitu
                    float sunshineHours = weekData.Count() > 0
                        ? (float)weekData.Count(h => h.SolarRad.HasValue && h.SolarRad.Value >= SUNSHINE_THRESHOLD) / weekData.Count() * 24 * 7
                        : 0;

                    result.Add(new SolarRadiationDataPoint
                    {
                        Date = weekStart,
                        DisplayTime = $"{weekStart:dd.MM} - {weekEnd:dd.MM}",
                        MinSolarRadiation = weekData.Where(w => w.SolarRad.HasValue).Min(w => w.SolarRad),
                        AvgSolarRadiation = weekData.Where(w => w.SolarRad.HasValue).Average(w => w.SolarRad),
                        MaxSolarRadiation = weekData.Where(w => w.SolarRad.HasValue).Max(w => w.SolarRad),
                        SunshineHours = sunshineHours
                    });
                }
                
                // Posuneme se na další týden
                currentDate = currentDate.AddDays(7);
            }

            return result;
        }

        private async Task<List<SolarRadiationDataPoint>> AggregateByMonthAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            var query = context.WeatherHistory.AsNoTracking()
                .Where(h => h.Date >= startDateUtc && h.Date <= endDateUtc)
                .OrderBy(h => h.Date);

            var weatherData = await query.ToListAsync();

            if (weatherData == null || !weatherData.Any())
            {
                _logger.LogWarning("Nenalezena žádná data slunečního záření pro období {StartDate} až {EndDate}", startDateUtc, endDateUtc);
                return new List<SolarRadiationDataPoint>();
            }

            var result = weatherData
                .GroupBy(h => new { Month = new DateTime(h.Date.Year, h.Date.Month, 1) })
                .Select(g => new SolarRadiationDataPoint
                {
                    Date = g.Key.Month,
                    DisplayTime = g.Key.Month.ToString("MMM yyyy"),
                    MinSolarRadiation = g.Where(h => h.SolarRad.HasValue).Min(h => h.SolarRad),
                    AvgSolarRadiation = g.Where(h => h.SolarRad.HasValue).Average(h => h.SolarRad),
                    MaxSolarRadiation = g.Where(h => h.SolarRad.HasValue).Max(h => h.SolarRad),
                    SunshineHours = g.Count() > 0 
                        ? (float)g.Count(h => h.SolarRad.HasValue && h.SolarRad.Value >= SUNSHINE_THRESHOLD) / g.Count() * 
                          DateTime.DaysInMonth(g.Key.Month.Year, g.Key.Month.Month) * 24 
                        : 0
                })
                .OrderBy(h => h.Date)
                .ToList();

            return result;
        }
    }
} 