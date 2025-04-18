using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    // Definuje kompozitní index pro zajištění, že každý blog má jen jeden obsah pro každý jazyk
    [Index(nameof(BlogId), nameof(LanguageCode), IsUnique = true)]
    public class BlogContent
    {
        public int Id { get; set; }

        [Required]
        public int BlogId { get; set; }

        [Required]
        [StringLength(10)] // "cs", "en", "de", atd.
        public string LanguageCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Tags { get; set; }

        public bool IsPublished { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigační vlastnost
        [ForeignKey(nameof(BlogId))]
        public Blog Blog { get; set; } = null!;
    }
} 