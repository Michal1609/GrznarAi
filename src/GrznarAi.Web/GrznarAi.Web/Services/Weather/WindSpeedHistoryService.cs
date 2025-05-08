using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services.Weather
{
    public class WindSpeedDataPoint
    {
        public DateTime Date { get; set; }
        public object DisplayTime { get; set; }
        public float? MinWindSpeed { get; set; }
        public float? AvgWindSpeed { get; set; }
        public float? MaxWindSpeed { get; set; }
        public float? GustWindSpeed { get; set; }
    }

    public interface IWindSpeedHistoryService
    {
        Task<List<WindSpeedDataPoint>> GetWindSpeedDataAsync(DateTime startDate, DateTime endDate, string aggregationType);
    }

    public class WindSpeedHistoryService : IWindSpeedHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<WindSpeedHistoryService> _logger;
        private readonly TimeZoneInfo _localTimeZone;

        public WindSpeedHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<WindSpeedHistoryService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            // Získání lokální časové zóny
            _localTimeZone = TimeZoneInfo.Local;
            _logger.LogInformation("Použitá časová zóna: {TimeZone}", _localTimeZone.DisplayName);
        }

        public async Task<List<WindSpeedDataPoint>> GetWindSpeedDataAsync(DateTime startDate, DateTime endDate, string aggregationType)
        {
            _logger.LogInformation("GetWindSpeedDataAsync - Načítání dat rychlosti větru: {StartDate} až {EndDate}, typ agregace: {AggregationType}", 
                startDate, endDate, aggregationType);

            try
            {
                // Konverze vstupních parametrů na UTC pro dotazy do databáze
                DateTime startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
                DateTime endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

                _logger.LogInformation("GetWindSpeedDataAsync - Konvertované UTC časy: {StartDateUtc} až {EndDateUtc}", 
                    startDateUtc, endDateUtc);
                
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var result = new List<WindSpeedDataPoint>();
                
                switch (aggregationType.ToLower())
                {
                    case "hourly":
                        result = await AggregateHourlyWindSpeedDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "daily":
                        result = await AggregateDailyWindSpeedDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "monthly":
                        result = await AggregateMonthlyWindSpeedDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    default:
                        _logger.LogWarning("Neznámý typ agregace: {AggregationType}, použije se hodinová agregace", aggregationType);
                        result = await AggregateHourlyWindSpeedDataAsync(context, startDateUtc, endDateUtc);
                        break;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání dat rychlosti větru: {Message}", ex.Message);
                return new List<WindSpeedDataPoint>();
            }
        }

        private async Task<List<WindSpeedDataPoint>> AggregateHourlyWindSpeedDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.WindSpeedAvg,
                h.WindSpeedHi
            }).ToList();

            // Seskupíme data v paměti podle lokálního času
            var result = localData
                .GroupBy(w => new
                {
                    Year = w.LocalDate.Year,
                    Month = w.LocalDate.Month,
                    Day = w.LocalDate.Day,
                    Hour = w.LocalDate.Hour
                })
                .Select(g => new WindSpeedDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0).ToString("HH:00"),
                    MinWindSpeed = g.Min(w => w.WindSpeedAvg),
                    AvgWindSpeed = g.Average(w => w.WindSpeedAvg),
                    MaxWindSpeed = g.Max(w => w.WindSpeedAvg),
                    GustWindSpeed = g.Max(w => w.WindSpeedHi)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<WindSpeedDataPoint>> AggregateDailyWindSpeedDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.WindSpeedAvg,
                h.WindSpeedHi
            }).ToList();

            // Seskupíme data v paměti podle lokálního času
            var result = localData
                .GroupBy(w => new
                {
                    Year = w.LocalDate.Year,
                    Month = w.LocalDate.Month,
                    Day = w.LocalDate.Day
                })
                .Select(g => new WindSpeedDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day).ToString("dd.MM."),
                    MinWindSpeed = g.Min(w => w.WindSpeedAvg),
                    AvgWindSpeed = g.Average(w => w.WindSpeedAvg),
                    MaxWindSpeed = g.Max(w => w.WindSpeedAvg),
                    GustWindSpeed = g.Max(w => w.WindSpeedHi)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<WindSpeedDataPoint>> AggregateMonthlyWindSpeedDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.WindSpeedAvg,
                h.WindSpeedHi
            }).ToList();

            // Seskupíme data v paměti podle lokálního času
            var result = localData
                .GroupBy(w => new
                {
                    Year = w.LocalDate.Year,
                    Month = w.LocalDate.Month
                })
                .Select(g => new WindSpeedDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MM.yyyy"),
                    MinWindSpeed = g.Min(w => w.WindSpeedAvg),
                    AvgWindSpeed = g.Average(w => w.WindSpeedAvg),
                    MaxWindSpeed = g.Max(w => w.WindSpeedAvg),
                    GustWindSpeed = g.Max(w => w.WindSpeedHi)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }
    }
} 