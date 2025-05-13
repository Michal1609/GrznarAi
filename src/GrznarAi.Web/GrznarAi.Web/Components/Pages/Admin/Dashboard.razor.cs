using GrznarAi.Web.Data;
using GrznarAi.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private ILogger<Dashboard> Logger { get; set; }

        private bool _isLoading = true;
        private int _aiNewsCount = 0;
        private int _usersCount = 0;
        private long _cacheTotalSize = 0;
        private int _cacheItemsCount = 0;
        private DateTime? _lastAiNewsDate = null;
        private DateTime? _lastWeatherDate = null;
        
        protected override async Task OnInitializedAsync()
        {
            await RefreshDashboardAsync();
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
                Logger.LogError(ex, "Chyba při načítání dat pro dashboard");
            }
            finally
            {
                _isLoading = false;
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