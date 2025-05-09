using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

                // Agregace dat podle zvoleného typu
                var uvIndexData = new List<UVIndexDataPoint>();

                switch (aggregationType.ToLower())
                {
                    case "hourly":
                        uvIndexData = AggregateByHour(weatherData);
                        break;
                    case "daily":
                        uvIndexData = AggregateByDay(weatherData);
                        break;
                    case "monthly":
                        uvIndexData = await AggregateMonthlyUVIndexDataAsync(context, startDateUtc, endDateUtc);
                        break;
                    default:
                        uvIndexData = AggregateByHour(weatherData);
                        break;
                }

                return uvIndexData;
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
    }
} 