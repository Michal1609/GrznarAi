using System;
using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Data
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Url]
        [StringLength(200)]
        public string? GitHubUrl { get; set; }

        [Required] 
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional: Add UpdatedAt
        // public DateTime? UpdatedAt { get; set; }
    }
} 