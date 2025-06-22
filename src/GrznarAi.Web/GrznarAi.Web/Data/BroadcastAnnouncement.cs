using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Data
{
    /// <summary>
    /// Represents a broadcast announcement from the local radio station
    /// </summary>
    public class BroadcastAnnouncement
    {
        /// <summary>
        /// Unique identifier for the announcement
        /// </summary>
        public int Id { get; set; }

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
        /// Date and time when the announcement was imported to the system
        /// </summary>
        public DateTime ImportedDateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates if the announcement is active/visible
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// URL of the audio file for the announcement
        /// </summary>
        [MaxLength(2048)]
        public string? AudioUrl { get; set; }
    }
} 