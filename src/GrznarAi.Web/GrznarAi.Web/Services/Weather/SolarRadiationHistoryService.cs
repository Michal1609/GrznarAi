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

                // Agregace dat podle zvoleného typu
                var solarRadiationData = new List<SolarRadiationDataPoint>();

                switch (aggregationType.ToLower())
                {
                    case "hourly":
                        solarRadiationData = AggregateByHour(weatherData);
                        break;
                    case "daily":
                        solarRadiationData = AggregateByDay(weatherData);
                        break;
                    case "monthly":
                        solarRadiationData = AggregateByMonth(weatherData);
                        break;
                    default:
                        _logger.LogWarning("Neznámý typ agregace: {AggregationType}, použiji hodinovou agregaci", aggregationType);
                        solarRadiationData = AggregateByHour(weatherData);
                        break;
                }

                return solarRadiationData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání dat slunečního záření: {Message}", ex.Message);
                return new List<SolarRadiationDataPoint>();
            }
        }

        private List<SolarRadiationDataPoint> AggregateByHour(List<WeatherHistory> weatherData)
        {
            var result = weatherData
                .GroupBy(h => new { Hour = new DateTime(h.Date.Year, h.Date.Month, h.Date.Day, h.Date.Hour, 0, 0) })
                .Select(g => new SolarRadiationDataPoint
                {
                    Date = g.Key.Hour,
                    DisplayTime = g.Key.Hour.ToString("HH:00"),
                    MinSolarRadiation = g.Where(h => h.SolarRad.HasValue).Min(h => h.SolarRad),
                    AvgSolarRadiation = g.Where(h => h.SolarRad.HasValue).Average(h => h.SolarRad),
                    MaxSolarRadiation = g.Where(h => h.SolarRad.HasValue).Max(h => h.SolarRad),
                    // Výpočet hodin slunečního svitu (poměr měření nad prahem / počet měření * délka agregačního období v hodinách)
                    SunshineHours = g.Count() > 0 
                        ? (float)g.Count(h => h.SolarRad.HasValue && h.SolarRad.Value >= SUNSHINE_THRESHOLD) / g.Count() 
                        : 0
                })
                .OrderBy(h => h.Date)
                .ToList();

            return result;
        }

        private List<SolarRadiationDataPoint> AggregateByDay(List<WeatherHistory> weatherData)
        {
            var result = weatherData
                .GroupBy(h => new { Day = new DateTime(h.Date.Year, h.Date.Month, h.Date.Day) })
                .Select(g => new SolarRadiationDataPoint
                {
                    Date = g.Key.Day,
                    DisplayTime = g.Key.Day.ToString("dd.MM."),
                    MinSolarRadiation = g.Where(h => h.SolarRad.HasValue).Min(h => h.SolarRad),
                    AvgSolarRadiation = g.Where(h => h.SolarRad.HasValue).Average(h => h.SolarRad),
                    MaxSolarRadiation = g.Where(h => h.SolarRad.HasValue).Max(h => h.SolarRad),
                    // Výpočet hodin slunečního svitu (počet záznamů nad prahem / počet záznamů za hodinu * 24)
                    SunshineHours = g.Count() > 0 
                        ? (float)g.Count(h => h.SolarRad.HasValue && h.SolarRad.Value >= SUNSHINE_THRESHOLD) / g.Count() * 24 
                        : 0
                })
                .OrderBy(h => h.Date)
                .ToList();

            return result;
        }

        private List<SolarRadiationDataPoint> AggregateByMonth(List<WeatherHistory> weatherData)
        {
            var result = weatherData
                .GroupBy(h => new { Month = new DateTime(h.Date.Year, h.Date.Month, 1) })
                .Select(g => new SolarRadiationDataPoint
                {
                    Date = g.Key.Month,
                    DisplayTime = g.Key.Month.ToString("MMM yyyy"),
                    MinSolarRadiation = g.Where(h => h.SolarRad.HasValue).Min(h => h.SolarRad),
                    AvgSolarRadiation = g.Where(h => h.SolarRad.HasValue).Average(h => h.SolarRad),
                    MaxSolarRadiation = g.Where(h => h.SolarRad.HasValue).Max(h => h.SolarRad),
                    // Výpočet hodin slunečního svitu
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