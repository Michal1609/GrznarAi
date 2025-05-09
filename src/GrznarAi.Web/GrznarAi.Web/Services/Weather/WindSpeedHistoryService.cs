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

            // Konverze vstupních parametrů na UTC pro dotazy do databáze
            DateTime startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            DateTime endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

            _logger.LogInformation("GetWindSpeedDataAsync - Konvertované UTC časy: {StartDateUtc} až {EndDateUtc}", 
                startDateUtc, endDateUtc);
            
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Podle typu agregace zvolíme způsob, jakým budou data seskupena
            var result = aggregationType switch
            {
                "hourly" => await AggregateHourlyWindSpeedDataAsync(context, startDateUtc, endDateUtc),
                "6hour" => await Aggregate6HourWindSpeedDataAsync(context, startDateUtc, endDateUtc),
                "daily" => await AggregateDailyWindSpeedDataAsync(context, startDateUtc, endDateUtc),
                "weekly" => await AggregateWeeklyWindSpeedDataAsync(context, startDateUtc, endDateUtc),
                "monthly" => await AggregateMonthlyWindSpeedDataAsync(context, startDateUtc, endDateUtc),
                _ => await AggregateHourlyWindSpeedDataAsync(context, startDateUtc, endDateUtc)
            };
            
            return result;
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

        private async Task<List<WindSpeedDataPoint>> Aggregate6HourWindSpeedDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
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

                    return new WindSpeedDataPoint
                    {
                        Date = blockDate,
                        DisplayTime = $"{blockDate:dd.MM} {startHour:00}:00-{startHour+5:00}:59",
                        MinWindSpeed = g.Min(w => w.WindSpeedAvg),
                        AvgWindSpeed = g.Average(w => w.WindSpeedAvg),
                        MaxWindSpeed = g.Max(w => w.WindSpeedAvg),
                        GustWindSpeed = g.Max(w => w.WindSpeedHi)
                    };
                })
                .OrderBy(r => r.Date)
                .ToList();

            return result;
        }

        private async Task<List<WindSpeedDataPoint>> AggregateWeeklyWindSpeedDataAsync(ApplicationDbContext context, DateTime startDateUtc, DateTime endDateUtc)
        {
            // Načteme data a zpracujeme je na klientské straně
            var data = await context.WeatherHistory
                .Where(w => w.Date >= startDateUtc && w.Date <= endDateUtc)
                .ToListAsync();

            // Konverze UTC času zpět na lokální pro správné týdenní zobrazení
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, _localTimeZone);
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, _localTimeZone);
            
            // Pro týdenní agregaci vytvoříme vlastní logiku pro seskupení po týdnech
            var result = new List<WindSpeedDataPoint>();
            
            // Připravíme kalendářní týdny v lokálním čase
            var currentDate = startDateLocal.Date;
            while (currentDate <= endDateLocal)
            {
                var weekStart = currentDate;
                var weekEnd = weekStart.AddDays(6) > endDateLocal ? endDateLocal : weekStart.AddDays(6);

                _logger.LogInformation("AggregateWeeklyWindSpeedDataAsync - Načítání dat pro týden: {WeekStart:dd.MM.yyyy} až {WeekEnd:dd.MM.yyyy}", 
                    weekStart, weekEnd);
                
                // Konverze zpět na UTC pro dotaz do databáze
                var weekStartUtc = DateTime.SpecifyKind(weekStart, DateTimeKind.Local).ToUniversalTime();
                var weekEndUtc = DateTime.SpecifyKind(weekEnd.AddHours(23).AddMinutes(59).AddSeconds(59), DateTimeKind.Local).ToUniversalTime();

                // Filtrujeme data pro tento týden
                var weekData = data.Where(w => w.Date >= weekStartUtc && w.Date <= weekEndUtc).ToList();
                    
                if (weekData.Any())
                {
                    result.Add(new WindSpeedDataPoint
                    {
                        Date = weekStart,
                        DisplayTime = $"{weekStart:dd.MM} - {weekEnd:dd.MM}",
                        MinWindSpeed = weekData.Min(w => w.WindSpeedAvg),
                        AvgWindSpeed = weekData.Average(w => w.WindSpeedAvg),
                        MaxWindSpeed = weekData.Max(w => w.WindSpeedAvg),
                        GustWindSpeed = weekData.Max(w => w.WindSpeedHi)
                    });
                }
                
                // Posuneme se na další týden
                currentDate = currentDate.AddDays(7);
            }

            return result;
        }
    }
} 