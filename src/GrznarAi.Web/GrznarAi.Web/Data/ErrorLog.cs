using System;
using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Data
{
    /// <summary>
    /// Universal error/warning log entity for application logging
    /// </summary>
    public class ErrorLog
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Log message (required)
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Stack trace (optional)
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Inner exception message (optional)
        /// </summary>
        public string? InnerException { get; set; }

        /// <summary>
        /// Log level (Error, Warning, Info, etc.)
        /// </summary>
        [StringLength(20)]
        public string? Level { get; set; }

        /// <summary>
        /// Source of the log (service/component name)
        /// </summary>
        [StringLength(100)]
        public string? Source { get; set; }

        /// <summary>
        /// Date and time when the log was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 