using GrznarAi.Web.Data;
using GrznarAi.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Components.Pages.Admin
{
    public partial class Dashboard
    {
        [Inject]
        private ILocalizationService Localizer { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private ICacheService CacheService { get; set; }

        [Inject]
        private IAiNewsService AiNewsService { get; set; }

        [Inject]
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; }

        [Inject]
        private UserManager<ApplicationUser> UserManager { get; set; }

        [Inject]
        private IWeatherHistoryService WeatherHistoryService { get; set; }

        [Inject]
        private IErrorLogService ErrorLogService { get; set; }

        [Inject] 
        private IJSRuntime JSRuntime { get; set; }

        private bool _isLoading = true;
        private bool _dataRefreshed = false;
        private int _aiNewsCount = 0;
        private int _usersCount = 0;
        private long _cacheTotalSize = 0;
        private int _cacheItemsCount = 0;
        private DateTime? _lastAiNewsDate = null;
        private DateTime? _lastWeatherDate = null;
        private decimal _dbSizeMB = 0;
        private decimal _dbSizeGB => Math.Round(_dbSizeMB / 1024, 2);
        private decimal _dbSizeLimit = 10; // Limit v GB, nastavte podle vašeho plánu
        private decimal _dbUsagePercentage => Math.Min((_dbSizeGB / _dbSizeLimit) * 100, 100);
        private long _pagesSize = 0;
        
        // Data pro koláčový graf využití DB
        private object[] _dbUsageChartData => new object[] { _dbSizeGB, Math.Max(0, _dbSizeLimit - _dbSizeGB) };
        private string[] _dbUsageChartLabels => new string[] { "Využito", "Volné" };
        private string[] _dbUsageChartColors => new string[] { "#39a3ff", "#e5e5e5" };
        
        protected override async Task OnInitializedAsync()
        {
            await RefreshDashboardAsync();
        }
        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!_isLoading && (_dataRefreshed || firstRender))
            {
                try
                {
                    _dataRefreshed = false;
                    
                    // Připravíme data pro graf
                    var chartData = new
                    {
                        series = _dbUsageChartData,
                        labels = _dbUsageChartLabels,
                        colors = _dbUsageChartColors
                    };
                    
                    // Voláme JavaScript pouze v OnAfterRenderAsync
                    await JSRuntime.InvokeVoidAsync("renderDatabaseUsageChart", "database-usage-chart", chartData);
                }
                catch (Exception ex)
                {
                    await ErrorLogService.LogAsync($"Chyba při vykreslování grafu databáze: {ex.Message}", ex.StackTrace, ex.InnerException?.Message, "Error", nameof(Dashboard));
                }
            }
        }
        
        private async Task RefreshDashboardAsync()
        {
            try
            {
                _isLoading = true;
                StateHasChanged();
                
                // Získání počtu AI novinek
                var aiNews = await AiNewsService.GetAiNewsAsync(1, 1);
                _aiNewsCount = aiNews.TotalCount;
                
                // Získání posledního datumu AI novinky
                using (var context = await DbContextFactory.CreateDbContextAsync())
                {
                    _lastAiNewsDate = await context.AiNewsItems
                        .OrderByDescending(n => n.PublishedDate)
                        .Select(n => n.PublishedDate)
                        .FirstOrDefaultAsync();
                        
                    // Získání velikosti databáze
                    var result = await context.Database
                        .SqlQueryRaw<decimal>(@"
                            SELECT
                                ROUND(CAST(SUM(size) * 8 AS DECIMAL(18,2)) / 1024, 2) AS total_size_mb
                            FROM
                                sys.master_files
                            WHERE
                                database_id = DB_ID()
                        ")
                        .ToListAsync();
                        
                    if (result.Count > 0)
                    {
                        _dbSizeMB = result[0];
                    }
                    
                    // Pokud je to možné, získání velikosti stránek
                    try
                    {
                        var pagesResult = await context.Database
                            .SqlQueryRaw<long>(@"
                                SELECT 
                                    SUM(p.reserved_page_count * 8192) AS TotalBytes
                                FROM 
                                    sys.dm_db_partition_stats p
                            ")
                            .ToListAsync();
                            
                        if (pagesResult.Count > 0)
                        {
                            _pagesSize = pagesResult[0];
                        }
                    }
                    catch (Exception ex)
                    {
                        await ErrorLogService.LogAsync($"Nepodařilo se získat informace o velikosti stránek: {ex.Message}", ex.StackTrace, ex.InnerException?.Message, "Warning", nameof(Dashboard));
                    }
                }
                
                // Získání počtu uživatelů
                _usersCount = await UserManager.Users.CountAsync();
                
                // Získání velikosti cache
                var cacheItems = await CacheService.GetCacheInfoAsync();
                _cacheTotalSize = cacheItems.Sum(i => i.Size);
                _cacheItemsCount = cacheItems.Count();
                
                // Získání posledního záznamu počasí
                _lastWeatherDate = await WeatherHistoryService.GetLastRecordDateAsync();
            }
            catch (Exception ex)
            {
                await ErrorLogService.LogAsync($"Chyba při načítání dat pro dashboard: {ex.Message}", ex.StackTrace, ex.InnerException?.Message, "Error", nameof(Dashboard));
            }
            finally
            {
                _isLoading = false;
                _dataRefreshed = true;
                StateHasChanged();
            }
        }
        
        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
    }
} 