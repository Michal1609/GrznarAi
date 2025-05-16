using System;
using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Api.Models.AiNews
{
    /// <summary>
    /// Model pro přidání AI novinky přes API
    /// </summary>
    public class AiNewsItemRequest
    {
        /// <summary>
        /// Titulek v angličtině
        /// </summary>
        [Required]
        public string TitleEn { get; set; } = string.Empty;

        /// <summary>
        /// Titulek v češtině
        /// </summary>
        [Required]
        public string TitleCz { get; set; } = string.Empty;

        /// <summary>
        /// Obsah v angličtině
        /// </summary>
        public string? ContentEn { get; set; }

        /// <summary>
        /// Obsah v češtině
        /// </summary>
        public string? ContentCz { get; set; }

        /// <summary>
        /// Souhrn v angličtině
        /// </summary>        
        public string? SummaryEn { get; set; }

        /// <summary>
        /// Souhrn v češtině
        /// </summary>
        public string? SummaryCz { get; set; }

        /// <summary>
        /// URL originálního zdroje
        /// </summary>
        [Required]
        [Url]
        [StringLength(500)]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// URL obrázku
        /// </summary>
        [Url]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Název zdroje
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SourceName { get; set; } = string.Empty;

        /// <summary>
        /// Datum publikace
        /// </summary>
        public DateTime? PublishedDate { get; set; }
    }
} 