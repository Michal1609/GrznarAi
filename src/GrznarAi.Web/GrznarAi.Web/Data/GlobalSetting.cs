using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    [Index(nameof(Key), IsUnique = true)]
    public class GlobalSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)] // Omezení délky klíče
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        // Volitelné pole pro typ hodnoty (string, int, bool, atd.)
        [StringLength(50)]
        public string? DataType { get; set; }

        // Volitelný popis pro administrační rozhraní
        public string? Description { get; set; }

        // Časové značky
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 