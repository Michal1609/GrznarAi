using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services.Weather
{
    public class WindDirectionDataPoint
    {
        public DateTime Date { get; set; }
        public object DisplayTime { get; set; }
        public float? WindDirection { get; set; }
    }

    public interface IWindDirectionHistoryService
    {
        Task<List<WindDirectionDataPoint>> GetWindDirectionDataAsync(DateTime startDate, DateTime endDate, string aggregationType);
    }

    public class WindDirectionHistoryService : IWindDirectionHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<WindDirectionHistoryService> _logger;
        private readonly TimeZoneInfo _localTimeZone;

        public WindDirectionHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<WindDirectionHistoryService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            // Získání lokální časové zóny
            _localTimeZone = TimeZoneInfo.Local;
            _logger.LogInformation("Použitá časová zóna: {TimeZone}", _localTimeZone.DisplayName);
        }

        public async Task<List<WindDirectionDataPoint>> GetWindDirectionDataAsync(DateTime startDate, DateTime endDate, string aggregationType)
        {
            _logger.LogInformation("GetWindDirectionDataAsync - Načítání dat směru větru: {StartDate} až {EndDate}, typ agregace: {AggregationType}", 
                startDate, endDate, aggregationType);

            try
            {
                // Konverze vstupních parametrů na UTC pro dotazy do databáze
                DateTime startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
                DateTime endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

                _logger.LogInformation("GetWindDirectionDataAsync - Konvertované UTC časy: {StartDateUtc} až {EndDateUtc}", 
                    startDateUtc, endDateUtc);
                
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var result = new List<WindDirectionDataPoint>();
                
                switch (aggregationType.ToLower())
                {
                    case "hourly":
                        result = await AggregateHourlyWindDirectionDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "daily":
                        result = await AggregateDailyWindDirectionDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    case "monthly":
                        result = await AggregateMonthlyWindDirectionDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    default:
                        _logger.LogWarning("Neznámý typ agregace: {AggregationType}, použije se hodinová agregace", aggregationType);
                        result = await AggregateHourlyWindDirectionDataAsync(context, startDateUtc, endDateUtc);
                        break;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání dat směru větru: {Message}", ex.Message);
                return new List<WindDirectionDataPoint>();
            }
        }

        private async Task<List<WindDirectionDataPoint>> AggregateHourlyWindDirectionDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc && w.WindDirection.HasValue)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.WindDirection
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
                .Select(g => new WindDirectionDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0).ToString("HH:00"),
                    // Pro směr větru můžeme použít průměr pro vizualizaci převládajícího směru v daném časovém úseku
                    WindDirection = g.Average(w => w.WindDirection)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<WindDirectionDataPoint>> AggregateDailyWindDirectionDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc && w.WindDirection.HasValue)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.WindDirection
            }).ToList();

            // Seskupíme data v paměti podle lokálního času
            var result = localData
                .GroupBy(w => new
                {
                    Year = w.LocalDate.Year,
                    Month = w.LocalDate.Month,
                    Day = w.LocalDate.Day
                })
                .Select(g => new WindDirectionDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day).ToString("dd.MM."),
                    WindDirection = g.Average(w => w.WindDirection)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<WindDirectionDataPoint>> AggregateMonthlyWindDirectionDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc && w.WindDirection.HasValue)
                .ToListAsync();

            // Nejprve konvertujeme UTC časy na lokální časy
            var localData = data.Select(h => new 
            {
                LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(h.Date, DateTimeKind.Utc), _localTimeZone),
                h.WindDirection
            }).ToList();

            // Seskupíme data v paměti podle lokálního času
            var result = localData
                .GroupBy(w => new
                {
                    Year = w.LocalDate.Year,
                    Month = w.LocalDate.Month
                })
                .Select(g => new WindDirectionDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    DisplayTime = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MM.yyyy"),
                    WindDirection = g.Average(w => w.WindDirection)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }
    }
} 