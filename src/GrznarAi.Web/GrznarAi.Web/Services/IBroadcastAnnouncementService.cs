using GrznarAi.Web.Api.Models;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Interface for broadcast announcement service
    /// </summary>
    public interface IBroadcastAnnouncementService
    {
        /// <summary>
        /// Get paged list of active broadcast announcements
        /// </summary>
        /// <param name="page">Page number (starts from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="search">Search term (fulltext)</param>
        /// <param name="day">Day to filter announcements (date only)</param>
        /// <returns>Paged list of announcements</returns>
        Task<PagedBroadcastAnnouncementResponse> GetPagedAnnouncementsAsync(int page = 1, int? pageSize = null, string? search = null, DateTime? day = null);

        /// <summary>
        /// Get paged list of all broadcast announcements for admin
        /// </summary>
        /// <param name="page">Page number (starts from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="search">Search term</param>
        /// <returns>Paged list of announcements</returns>
        Task<PagedBroadcastAnnouncementResponse> GetPagedAnnouncementsForAdminAsync(int page = 1, int pageSize = 20, string? search = null);

        /// <summary>
        /// Delete broadcast announcement by ID
        /// </summary>
        /// <param name="id">Announcement ID</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteAnnouncementAsync(int id);

        /// <summary>
        /// Delete all broadcast announcements
        /// </summary>
        /// <returns>Number of deleted announcements</returns>
        Task<int> DeleteAllAnnouncementsAsync();
    }
} 