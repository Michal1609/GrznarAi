using System;
using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Api.Models
{
    /// <summary>
    /// Reprezentuje API klíč pro přístup k API
    /// </summary>
    public class ApiKey
    {
        /// <summary>
        /// ID API klíče
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Název API klíče (pro identifikaci)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Hodnota API klíče
        /// </summary>
        [StringLength(100)]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Datum vytvoření klíče
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Datum poslední aktualizace klíče
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Datum expirace klíče (pokud je nastaveno)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Zda je klíč aktivní
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Popis klíče
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }
    }
} 