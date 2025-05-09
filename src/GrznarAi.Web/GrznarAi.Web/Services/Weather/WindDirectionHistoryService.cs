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

            // Konverze vstupních parametrů na UTC pro dotazy do databáze
            DateTime startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            DateTime endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

            _logger.LogInformation("GetWindDirectionDataAsync - Konvertované UTC časy: {StartDateUtc} až {EndDateUtc}", 
                startDateUtc, endDateUtc);
            
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Podle typu agregace zvolíme způsob, jakým budou data seskupena
            var result = aggregationType switch
            {
                "hourly" => await AggregateHourlyWindDirectionDataAsync(context, startDateUtc, endDateUtc),
                "6hour" => await Aggregate6HourWindDirectionDataAsync(context, startDateUtc, endDateUtc),
                "daily" => await AggregateDailyWindDirectionDataAsync(context, startDateUtc, endDateUtc),
                "weekly" => await AggregateWeeklyWindDirectionDataAsync(context, startDateUtc, endDateUtc),
                "monthly" => await AggregateMonthlyWindDirectionDataAsync(context, startDateUtc, endDateUtc),
                _ => await AggregateHourlyWindDirectionDataAsync(context, startDateUtc, endDateUtc)
            };
            
            return result;
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

        private async Task<List<WindDirectionDataPoint>> Aggregate6HourWindDirectionDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
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

            // Seskupíme data v paměti podle lokálního času a 6hodinových bloků
            var result = localData
                .GroupBy(w => new
                {
                    Year = w.LocalDate.Year,
                    Month = w.LocalDate.Month,
                    Day = w.LocalDate.Day,
                    SixHourBlock = w.LocalDate.Hour / 6
                })
                .Select(g =>
                {
                    // Vypočítáme začátek 6hodinového bloku
                    int startHour = g.Key.SixHourBlock * 6;
                    var blockDate = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, startHour, 0, 0);

                    return new WindDirectionDataPoint
                    {
                        Date = blockDate,
                        DisplayTime = $"{blockDate:dd.MM} {startHour:00}:00-{startHour+5:00}:59",
                        // Pro směr větru můžeme použít průměr pro vizualizaci převládajícího směru v daném časovém úseku
                        WindDirection = g.Average(w => w.WindDirection)
                    };
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<WindDirectionDataPoint>> AggregateWeeklyWindDirectionDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc && w.WindDirection.HasValue)
                .ToListAsync();

            // Konverze UTC času zpět na lokální pro správné týdenní zobrazení
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, _localTimeZone);
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, _localTimeZone);
            
            // Pro týdenní agregaci vytvoříme vlastní logiku pro seskupení po týdnech
            var result = new List<WindDirectionDataPoint>();
            
            // Připravíme kalendářní týdny v lokálním čase
            var currentDate = startDateLocal.Date;
            while (currentDate <= endDateLocal)
            {
                var weekStart = currentDate;
                var weekEnd = weekStart.AddDays(6) > endDateLocal ? endDateLocal : weekStart.AddDays(6);

                _logger.LogInformation("AggregateWeeklyWindDirectionDataAsync - Načítání dat pro týden: {WeekStart:dd.MM.yyyy} až {WeekEnd:dd.MM.yyyy}", 
                    weekStart, weekEnd);
                
                // Konverze zpět na UTC pro dotaz do databáze
                var weekStartUtc = DateTime.SpecifyKind(weekStart, DateTimeKind.Local).ToUniversalTime();
                var weekEndUtc = DateTime.SpecifyKind(weekEnd.AddHours(23).AddMinutes(59).AddSeconds(59), DateTimeKind.Local).ToUniversalTime();

                // Filtrujeme data pro tento týden
                var weekData = data.Where(w => w.Date >= weekStartUtc && w.Date <= weekEndUtc).ToList();
                    
                if (weekData.Any())
                {
                    result.Add(new WindDirectionDataPoint
                    {
                        Date = weekStart,
                        DisplayTime = $"{weekStart:dd.MM} - {weekEnd:dd.MM}",
                        // Pro směr větru použijeme průměr
                        WindDirection = weekData.Average(w => w.WindDirection)
                    });
                }
                
                // Posuneme se na další týden
                currentDate = currentDate.AddDays(7);
            }

            return result;
        }
    }
} 