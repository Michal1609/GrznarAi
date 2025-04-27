using System;
using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Data
{
    /// <summary>
    /// Typ zdroje, ze kterého se stahují novinky
    /// </summary>
    public enum SourceType
    {
        Web = 0,
        Facebook = 1,
        Twitter = 2
    }

    /// <summary>
    /// Zdroj AI novinek pro stahování z externích stránek
    /// </summary>
    public class AiNewsSource
    {
        /// <summary>
        /// Unikátní identifikátor zdroje
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Název zdroje (např. "TechCrunch", "Wired", "AI News")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// URL zdroje novinek
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Typ zdroje (web, Facebook, Twitter)
        /// </summary>
        public SourceType Type { get; set; } = SourceType.Web;

        /// <summary>
        /// Datum a čas posledního stažení novinek z tohoto zdroje
        /// </summary>
        public DateTime? LastFetched { get; set; }

        /// <summary>
        /// Datum a čas přidání zdroje
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Datum a čas poslední aktualizace zdroje
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Aktivní status zdroje - zda se má používat pro stahování
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Popis zdroje, poznámky k implementaci
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Případné parametry specifické pro tento zdroj (JSON formát)
        /// </summary>
        public string? Parameters { get; set; }
    }
} 