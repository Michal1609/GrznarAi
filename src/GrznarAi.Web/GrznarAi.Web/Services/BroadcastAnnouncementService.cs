using Microsoft.EntityFrameworkCore;
using GrznarAi.Web.Data;
using GrznarAi.Web.Api.Models;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Service for managing broadcast announcements
    /// </summary>
    public class BroadcastAnnouncementService : IBroadcastAnnouncementService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IGlobalSettingsService _globalSettingsService;
        private readonly ILogger<BroadcastAnnouncementService> _logger;

        public BroadcastAnnouncementService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IGlobalSettingsService globalSettingsService,
            ILogger<BroadcastAnnouncementService> logger)
        {
            _contextFactory = contextFactory;
            _globalSettingsService = globalSettingsService;
            _logger = logger;
        }

        /// <summary>
        /// Get paged list of active broadcast announcements
        /// </summary>
        public async Task<PagedBroadcastAnnouncementResponse> GetPagedAnnouncementsAsync(int page = 1, int? pageSize = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Get page size from global settings if not provided
                var actualPageSize = pageSize ?? _globalSettingsService.GetInt("BroadcastAnnouncements.PageSize", 10);
                
                // Ensure minimum values
                page = Math.Max(1, page);
                actualPageSize = Math.Max(1, Math.Min(100, actualPageSize)); // Max 100 items per page

                var query = context.BroadcastAnnouncements
                    .Where(ba => ba.IsActive)
                    .OrderByDescending(ba => ba.BroadcastDateTime);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / actualPageSize);

                var announcements = await query
                    .Skip((page - 1) * actualPageSize)
                    .Take(actualPageSize)
                    .Select(ba => new BroadcastAnnouncementResponse
                    {
                        Id = ba.Id,
                        Content = ba.Content,
                        BroadcastDateTime = ba.BroadcastDateTime,
                        ImportedDateTime = ba.ImportedDateTime,
                        IsActive = ba.IsActive,
                        AudioUrl = ba.AudioUrl
                    })
                    .ToListAsync();

                return new PagedBroadcastAnnouncementResponse
                {
                    Announcements = announcements,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = actualPageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving broadcast announcements");
                throw;
            }
        }

        /// <summary>
        /// Get paged list of all broadcast announcements for admin
        /// </summary>
        public async Task<PagedBroadcastAnnouncementResponse> GetPagedAnnouncementsForAdminAsync(int page = 1, int pageSize = 20, string? search = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Ensure minimum values
                page = Math.Max(1, page);
                pageSize = Math.Max(1, Math.Min(100, pageSize)); // Max 100 items per page

                var query = context.BroadcastAnnouncements.AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(ba => ba.Content.ToLower().Contains(searchLower));
                }

                query = query.OrderByDescending(ba => ba.BroadcastDateTime);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var announcements = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ba => new BroadcastAnnouncementResponse
                    {
                        Id = ba.Id,
                        Content = ba.Content,
                        BroadcastDateTime = ba.BroadcastDateTime,
                        ImportedDateTime = ba.ImportedDateTime,
                        IsActive = ba.IsActive,
                        AudioUrl = ba.AudioUrl
                    })
                    .ToListAsync();

                return new PagedBroadcastAnnouncementResponse
                {
                    Announcements = announcements,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving broadcast announcements for admin with search '{Search}'", search);
                throw;
            }
        }

        /// <summary>
        /// Delete broadcast announcement by ID
        /// </summary>
        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var announcement = await context.BroadcastAnnouncements
                    .FirstOrDefaultAsync(ba => ba.Id == id);

                if (announcement == null)
                {
                    return false;
                }

                context.BroadcastAnnouncements.Remove(announcement);
                await context.SaveChangesAsync();

                _logger.LogInformation("Deleted broadcast announcement with ID {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting broadcast announcement with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Delete all broadcast announcements
        /// </summary>
        public async Task<int> DeleteAllAnnouncementsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var announcements = await context.BroadcastAnnouncements.ToListAsync();
                var count = announcements.Count;

                if (count > 0)
                {
                    context.BroadcastAnnouncements.RemoveRange(announcements);
                    await context.SaveChangesAsync();

                    _logger.LogInformation("Deleted all {Count} broadcast announcements", count);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all broadcast announcements");
                throw;
            }
        }
    }
} 