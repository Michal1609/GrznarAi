using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services.Weather
{
    public class PressureDataPoint
    {
        public DateTime Date { get; set; }
        public object DisplayTime { get; set; }
        public float? MinPressure { get; set; }
        public float? AvgPressure { get; set; }
        public float? MaxPressure { get; set; }
    }

    public interface IPressureHistoryService
    {
        Task<List<PressureDataPoint>> GetPressureDataAsync(DateTime startDate, DateTime endDate, string aggregationType);
    }

    public class PressureHistoryService : IPressureHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<PressureHistoryService> _logger;
        private readonly TimeZoneInfo _localTimeZone;

        public PressureHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<PressureHistoryService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            // Získání lokální časové zóny
            _localTimeZone = TimeZoneInfo.Local;
            _logger.LogInformation("Použitá časová zóna: {TimeZone}", _localTimeZone.DisplayName);
        }

        public async Task<List<PressureDataPoint>> GetPressureDataAsync(DateTime startDate, DateTime endDate, string aggregationType)
        {
            _logger.LogInformation("GetPressureDataAsync - Načítání dat atmosférického tlaku: {StartDate} až {EndDate}, typ agregace: {AggregationType}", 
                startDate, endDate, aggregationType);

            try
            {
                // Konverze vstupních parametrů na UTC pro dotazy do databáze
                DateTime startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
                DateTime endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

                _logger.LogInformation("GetPressureDataAsync - Konvertované UTC časy: {StartDateUtc} až {EndDateUtc}", 
                    startDateUtc, endDateUtc);
                
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var result = new List<PressureDataPoint>();
                
                switch (aggregationType.ToLower())
                {
                    case "hourly":
                        result = await AggregateHourlyPressureDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "daily":
                        result = await AggregateDailyPressureDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "monthly":
                        result = await AggregateMonthlyPressureDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    default:
                        _logger.LogWarning("Neznámý typ agregace: {AggregationType}, použije se hodinová agregace", aggregationType);
                        result = await AggregateHourlyPressureDataAsync(context, startDateUtc, endDateUtc);
                        break;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání dat atmosférického tlaku: {Message}", ex.Message);
                return new List<PressureDataPoint>();
            }
        }

        private async Task<List<PressureDataPoint>> AggregateHourlyPressureDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.Bar
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
                .Select(g => new PressureDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0).ToString("HH:00"),
                    MinPressure = g.Min(w => w.Bar),
                    AvgPressure = g.Average(w => w.Bar),
                    MaxPressure = g.Max(w => w.Bar)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<PressureDataPoint>> AggregateDailyPressureDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.Bar
            }).ToList();

            // Seskupíme data v paměti podle lokálního času
            var result = localData
                .GroupBy(w => new
                {
                    Year = w.LocalDate.Year,
                    Month = w.LocalDate.Month,
                    Day = w.LocalDate.Day
                })
                .Select(g => new PressureDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day).ToString("dd.MM."),
                    MinPressure = g.Min(w => w.Bar),
                    AvgPressure = g.Average(w => w.Bar),
                    MaxPressure = g.Max(w => w.Bar)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<PressureDataPoint>> AggregateMonthlyPressureDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.Bar
            }).ToList();

            // Seskupíme data v paměti podle lokálního času
            var result = localData
                .GroupBy(w => new
                {
                    Year = w.LocalDate.Year,
                    Month = w.LocalDate.Month
                })
                .Select(g => new PressureDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MM.yyyy"),
                    MinPressure = g.Min(w => w.Bar),
                    AvgPressure = g.Average(w => w.Bar),
                    MaxPressure = g.Max(w => w.Bar)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }
    }
} 