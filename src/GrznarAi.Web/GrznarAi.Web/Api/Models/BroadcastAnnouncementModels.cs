using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Api.Models
{
    /// <summary>
    /// Request model for creating a new broadcast announcement
    /// </summary>
    public class CreateBroadcastAnnouncementRequest
    {
        /// <summary>
        /// Text content of the announcement
        /// </summary>
        [Required]
        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the announcement was broadcast
        /// </summary>
        [Required]
        public DateTime BroadcastDateTime { get; set; }

        /// <summary>
        /// Optional URL of the audio file for the announcement
        /// </summary>
        [MaxLength(2048)]
        public string? AudioUrl { get; set; }
    }

    /// <summary>
    /// Response model for broadcast announcement
    /// </summary>
    public class BroadcastAnnouncementResponse
    {
        /// <summary>
        /// Unique identifier for the announcement
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Text content of the announcement
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the announcement was broadcast
        /// </summary>
        public DateTime BroadcastDateTime { get; set; }

        /// <summary>
        /// Date and time when the announcement was imported to the system
        /// </summary>
        public DateTime ImportedDateTime { get; set; }

        /// <summary>
        /// Indicates if the announcement is active/visible
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// URL of the audio file for the announcement
        /// </summary>
        public string? AudioUrl { get; set; }
    }

    /// <summary>
    /// Paged response for broadcast announcements
    /// </summary>
    public class PagedBroadcastAnnouncementResponse
    {
        /// <summary>
        /// List of announcements
        /// </summary>
        public List<BroadcastAnnouncementResponse> Announcements { get; set; } = new();

        /// <summary>
        /// Total number of announcements
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indicates if there is a next page
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Indicates if there is a previous page
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
} 