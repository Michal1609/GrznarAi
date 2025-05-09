using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrznarAi.Web.Data;

namespace GrznarAi.Web.Services
{
    public interface IMeteoHistoryService
    {
        Task<List<DailyStatisticsForDate>> GetDailyStatisticsForLastYearsAsync(DateTime date, int years);
        Task<List<YearlyStatistics>> GetYearlyStatisticsAsync(int startYear, int endYear);
        Task<List<DailyStatisticsForDate>> RefreshDailyStatisticsForLastYearsAsync(DateTime date, int years);
        Task<List<YearlyStatistics>> RefreshYearlyStatisticsAsync(int startYear, int endYear);
    }

    public class MeteoHistoryService : IMeteoHistoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<MeteoHistoryService> _logger;
        private readonly ICacheService _cacheService;
        
        // Cache key templates
        private const string DAILY_STATS_CACHE_KEY = "DailyStats_{0}_{1}"; // format: DailyStats_yyyy-MM-dd_years
        private const string YEARLY_STATS_CACHE_KEY = "YearlyStats_{0}_{1}"; // format: YearlyStats_startYear_endYear
        
        // Cache expiration (1 hour for historical data as it doesn't change often)
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

        public MeteoHistoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<MeteoHistoryService> logger,
            ICacheService cacheService)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Získá denní statistiky pro zadané datum napříč posledními roky (z cache nebo databáze)
        /// </summary>
        /// <param name="date">Aktuální datum</param>
        /// <param name="years">Počet let zpětně</param>
        /// <returns>Seznam denních statistik pro každý rok pro stejný den a měsíc</returns>
        public async Task<List<DailyStatisticsForDate>> GetDailyStatisticsForLastYearsAsync(DateTime date, int years)
        {
            string cacheKey = string.Format(DAILY_STATS_CACHE_KEY, date.ToString("yyyy-MM-dd"), years);
            
            return await _cacheService.GetOrCreateAsync(cacheKey, 
                () => FetchDailyStatisticsForLastYearsAsync(date, years), 
                _cacheExpiration);
        }

        /// <summary>
        /// Vynutí obnovení denních statistik z databáze a aktualizuje cache
        /// </summary>
        public async Task<List<DailyStatisticsForDate>> RefreshDailyStatisticsForLastYearsAsync(DateTime date, int years)
        {
            string cacheKey = string.Format(DAILY_STATS_CACHE_KEY, date.ToString("yyyy-MM-dd"), years);
            
            // Fetch fresh data from database
            var statistics = await FetchDailyStatisticsForLastYearsAsync(date, years);
            
            // Update cache
            if (statistics != null)
            {
                await _cacheService.SetAsync(cacheKey, statistics, _cacheExpiration);
            }
            
            return statistics;
        }

        /// <summary>
        /// Získá roční statistiky pro zadané roky (z cache nebo databáze)
        /// </summary>
        /// <param name="startYear">Počáteční rok</param>
        /// <param name="endYear">Koncový rok</param>
        /// <returns>Seznam ročních statistik pro každý rok</returns>
        public async Task<List<YearlyStatistics>> GetYearlyStatisticsAsync(int startYear, int endYear)
        {
            string cacheKey = string.Format(YEARLY_STATS_CACHE_KEY, startYear, endYear);
            
            return await _cacheService.GetOrCreateAsync(cacheKey, 
                () => FetchYearlyStatisticsAsync(startYear, endYear), 
                _cacheExpiration);
        }

        /// <summary>
        /// Vynutí obnovení ročních statistik z databáze a aktualizuje cache
        /// </summary>
        public async Task<List<YearlyStatistics>> RefreshYearlyStatisticsAsync(int startYear, int endYear)
        {
            string cacheKey = string.Format(YEARLY_STATS_CACHE_KEY, startYear, endYear);
            
            // Fetch fresh data from database
            var statistics = await FetchYearlyStatisticsAsync(startYear, endYear);
            
            // Update cache
            if (statistics != null)
            {
                await _cacheService.SetAsync(cacheKey, statistics, _cacheExpiration);
            }
            
            return statistics;
        }

        /// <summary>
        /// Načte denní statistiky přímo z databáze
        /// </summary>
        private async Task<List<DailyStatisticsForDate>> FetchDailyStatisticsForLastYearsAsync(DateTime date, int years)
        {
            var result = new List<DailyStatisticsForDate>();
            
            try
            {
                // Omezení maximálního počtu let na 20
                if (years > 20)
                {
                    _logger.LogWarning("Požadovaný počet let ({RequestedYears}) překračuje povolený limit. Omezeno na 20 let.", years);
                    years = 20;
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // Určíme roky, pro které chceme získat data
                var currentYear = DateTime.Now.Year;
                var startYear = Math.Max(currentYear - years + 1, 2000); // +1 protože počítáme i aktuální rok
                
                // Získáme den a měsíc z aktuálního data
                var day = date.Day;
                var month = date.Month;
                
                // Pro každý rok získáme data, ale optimalizovaným způsobem
                for (int year = startYear; year <= currentYear; year++)
                {
                    // Vytvoříme datum pro daný rok
                    var targetDate = new DateTime(year, month, day);
                    
                    // SQL optimalizace: použijeme AsNoTracking() pro snížení paměťových nároků
                    // a efektivnější výpočet agregací přímo v SQL dotazu
                    var dayStats = await context.WeatherHistory
                        .Where(h => h.Date.Year == year && h.Date.Month == month && h.Date.Day == day)
                        .GroupBy(h => 1) // Seskupit všechny záznamy do jedné skupiny pro agregaci
                        .Select(g => new 
                        {
                            MinTemperature = g.Min(r => r.TemperatureOut),
                            MaxTemperature = g.Max(r => r.TemperatureOut),
                            AvgTemperature = g.Average(r => r.TemperatureOut),
                            TotalRainfall = g.Max(r => r.Rain),
                            AvgHumidity = g.Average(r => r.HumidityOut)
                        })
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                    
                    if (dayStats != null)
                    {
                        // Vypočítáme denní statistiky
                        var stats = new DailyStatisticsForDate
                        {
                            Date = targetDate,
                            Year = year,
                            MinTemperature = dayStats.MinTemperature,
                            MaxTemperature = dayStats.MaxTemperature,
                            AvgTemperature = dayStats.AvgTemperature,
                            TotalRainfall = dayStats.TotalRainfall,
                            AvgHumidity = dayStats.AvgHumidity
                        };
                        
                        result.Add(stats);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při získávání denních statistik pro datum {Date}", date);
            }
            
            return result.OrderByDescending(r => r.Year).ToList();
        }

        /// <summary>
        /// Načte roční statistiky přímo z databáze
        /// </summary>
        private async Task<List<YearlyStatistics>> FetchYearlyStatisticsAsync(int startYear, int endYear)
        {
            var result = new List<YearlyStatistics>();
            
            try
            {
                // Omezení rozsahu let pro zabránění přetížení paměti
                int maxYearRange = 20;
                if (endYear - startYear > maxYearRange)
                {
                    _logger.LogWarning("Požadovaný rozsah let ({RequestedRange}) překračuje povolený limit. Omezeno na {MaxRange} let.", 
                        endYear - startYear, maxYearRange);
                    endYear = startYear + maxYearRange;
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // Pro každý rok získáme statistiky, ale efektivnějším způsobem
                for (int year = startYear; year <= endYear; year++)
                {
                    var yearStart = new DateTime(year, 1, 1);
                    var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);
                    
                    // Získáme základní statistiky pro tento rok (efektivně přímo v SQL)
                    var yearStats = await context.WeatherHistory
                        .Where(h => h.Date >= yearStart && h.Date <= yearEnd)
                        .GroupBy(h => 1) // Seskupit všechny záznamy do jedné skupiny pro agregaci
                        .Select(g => new 
                        {
                            MinTemperature = g.Min(r => r.TemperatureOut),
                            MaxTemperature = g.Max(r => r.TemperatureOut),
                            AvgTemperature = g.Average(r => r.TemperatureOut)
                        })
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                    
                    if (yearStats != null)
                    {
                        // Vytvoříme statistiky pro daný rok
                        var stats = new YearlyStatistics
                        {
                            Year = year,
                            MinTemperature = yearStats.MinTemperature,
                            MaxTemperature = yearStats.MaxTemperature,
                            AvgTemperature = yearStats.AvgTemperature
                        };
                        
                        // Tyto údaje potřebují separátní výpočty, které nelze snadno sloučit do jednoho SQL dotazu
                        // Provádíme je jedna po druhé, abychom minimalizovali zátěž na databázi
                        
                        // Získáme celkové srážky - tento výpočet jsme optimalizovali
                        stats.TotalRainfall = await CalculateTotalYearlyRainfallAsync(year, context);
                        
                        // Tyto hodnoty získáváme efektivnějšími metodami
                        stats.LastFrostDayFirstHalf = await GetLastFrostDayInFirstHalfAsync(year, context);
                        stats.FirstFrostDaySecondHalf = await GetFirstFrostDayInSecondHalfAsync(year, context);
                        stats.FrostDaysCount = await GetNumberOfFrostDaysAsync(year, context);
                        
                        stats.FirstHotDay = await GetFirstHotDayAsync(year, context);
                        stats.LastHotDay = await GetLastHotDayAsync(year, context);
                        stats.HotDaysCount = await GetNumberOfHotDaysAsync(year, context);
                        
                        result.Add(stats);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při získávání ročních statistik pro roky {StartYear} až {EndYear}", startYear, endYear);
            }
            
            return result.OrderByDescending(r => r.Year).ToList();
        }

        /// <summary>
        /// Vypočítá celkové srážky za rok podle vzoru ze zadání
        /// </summary>
        private async Task<float?> CalculateTotalYearlyRainfallAsync(int year, ApplicationDbContext context)
        {
            // Nejprve získáme denní maxima srážek
            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);
            
            // Pomocí SQL query vytvoříme ekvivalent zadaného SQL požadavku
            var dailyMaxRainfall = await context.WeatherHistory
                .Where(h => h.Date >= yearStart && h.Date <= yearEnd && h.Rain > 0)
                .GroupBy(h => new { h.Date.Year, h.Date.Month, h.Date.Day })
                .Select(g => new { Day = g.Key, MaxRain = g.Max(h => h.Rain) })
                .ToListAsync();
            
            // Sečteme denní maxima pro získání celkového ročního úhrnu
            return dailyMaxRainfall.Sum(d => d.MaxRain);
        }

        /// <summary>
        /// Získá poslední den mrazu v první polovině roku
        /// </summary>
        private async Task<DateTime?> GetLastFrostDayInFirstHalfAsync(int year, ApplicationDbContext context)
        {
            var firstHalfStart = new DateTime(year, 1, 1);
            var firstHalfEnd = new DateTime(year, 6, 30, 23, 59, 59);
            
            // Získáme poslední den, kdy teplota klesla pod 0°C v první polovině roku
            return await context.WeatherHistory
                .Where(h => h.Date >= firstHalfStart && h.Date <= firstHalfEnd && h.TemperatureOut <= 0)
                .OrderByDescending(h => h.Date)
                .Select(h => h.Date)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Získá první den mrazu v druhé polovině roku
        /// </summary>
        private async Task<DateTime?> GetFirstFrostDayInSecondHalfAsync(int year, ApplicationDbContext context)
        {
            var secondHalfStart = new DateTime(year, 7, 1);
            var secondHalfEnd = new DateTime(year, 12, 31, 23, 59, 59);
            
            // Získáme první den, kdy teplota klesla pod 0°C v druhé polovině roku
            return await context.WeatherHistory
                .Where(h => h.Date >= secondHalfStart && h.Date <= secondHalfEnd && h.TemperatureOut <= 0)
                .OrderBy(h => h.Date)
                .Select(h => h.Date)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Získá počet mrazivých dnů v roce
        /// </summary>
        private async Task<int> GetNumberOfFrostDaysAsync(int year, ApplicationDbContext context)
        {
            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);
            
            // Zjistíme počet dní, kdy teplota klesla pod 0°C
            var frostDays = await context.WeatherHistory
                .Where(h => h.Date >= yearStart && h.Date <= yearEnd && h.TemperatureOut <= 0)
                .Select(h => new { h.Date.Year, h.Date.Month, h.Date.Day })
                .Distinct()
                .CountAsync();
            
            return frostDays;
        }

        /// <summary>
        /// Získá první horký den v roce (teplota >= 30°C)
        /// </summary>
        private async Task<DateTime?> GetFirstHotDayAsync(int year, ApplicationDbContext context)
        {
            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);
            
            // Získáme první den, kdy teplota dosáhla nebo překročila 30°C
            return await context.WeatherHistory
                .Where(h => h.Date >= yearStart && h.Date <= yearEnd && h.TemperatureOut >= 30)
                .OrderBy(h => h.Date)
                .Select(h => h.Date)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Získá poslední horký den v roce (teplota >= 30°C)
        /// </summary>
        private async Task<DateTime?> GetLastHotDayAsync(int year, ApplicationDbContext context)
        {
            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);
            
            // Získáme poslední den, kdy teplota dosáhla nebo překročila 30°C
            return await context.WeatherHistory
                .Where(h => h.Date >= yearStart && h.Date <= yearEnd && h.TemperatureOut >= 30)
                .OrderByDescending(h => h.Date)
                .Select(h => h.Date)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Získá počet horkých dnů v roce (teplota >= 30°C)
        /// </summary>
        private async Task<int> GetNumberOfHotDaysAsync(int year, ApplicationDbContext context)
        {
            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);
            
            // Zjistíme počet dní, kdy teplota dosáhla nebo překročila 30°C
            var hotDays = await context.WeatherHistory
                .Where(h => h.Date >= yearStart && h.Date <= yearEnd && h.TemperatureOut >= 30)
                .Select(h => new { h.Date.Year, h.Date.Month, h.Date.Day })
                .Distinct()
                .CountAsync();
            
            return hotDays;
        }
    }

    /// <summary>
    /// Model pro denní statistiky pro stejné datum v různých letech
    /// </summary>
    public class DailyStatisticsForDate
    {
        public DateTime Date { get; set; }
        public int Year { get; set; }
        public float? MinTemperature { get; set; }
        public float? AvgTemperature { get; set; }
        public float? MaxTemperature { get; set; }
        public float? TotalRainfall { get; set; }
        public float? AvgHumidity { get; set; }
    }

    /// <summary>
    /// Model pro roční statistiky
    /// </summary>
    public class YearlyStatistics
    {
        public int Year { get; set; }
        
        // Poslední den kdy mrzlo na začátku roku (poslední den kdy teplota < 0 a den < 30.6)
        public DateTime? LastFrostDayFirstHalf { get; set; }
        
        // První den kdy začalo mrznout v druhé polovině roku
        public DateTime? FirstFrostDaySecondHalf { get; set; }
        
        // Počet dní kdy bylo 0 stupňů a méně
        public int FrostDaysCount { get; set; }
        
        // První den v roce kdy bylo 30 stupňů
        public DateTime? FirstHotDay { get; set; }
        
        // Poslední den kdy bylo 30 stupňů v celém roce
        public DateTime? LastHotDay { get; set; }
        
        // Počet dní kdy bylo 30 stupňů a více
        public int HotDaysCount { get; set; }
        
        // Minimální teplota za celý rok
        public float? MinTemperature { get; set; }
        
        // Maximální teplota za celý rok
        public float? MaxTemperature { get; set; }
        
        // Průměrná roční teplota
        public float? AvgTemperature { get; set; }
        
        // Celkový úhrn srážek za celý rok
        public float? TotalRainfall { get; set; }
    }
} 