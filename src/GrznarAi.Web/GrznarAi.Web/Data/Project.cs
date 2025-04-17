using System;
using System.Collections.Generic;
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

        // Navigační vlastnost pro vícejazyčný obsah
        public ICollection<ProjectContent> Contents { get; set; } = new List<ProjectContent>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional: Add UpdatedAt
        // public DateTime? UpdatedAt { get; set; }
    }
} 