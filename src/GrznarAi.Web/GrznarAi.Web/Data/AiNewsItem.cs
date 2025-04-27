using System;
using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Data
{
    public class AiNewsItem
    {
        /// <summary>
        /// Unikátní identifikátor zprávy
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Titulek zprávy v angličtině
        /// </summary>
        [Required]
        public string TitleEn { get; set; } = string.Empty;

        /// <summary>
        /// Titulek zprávy v češtině
        /// </summary>
        [Required]
        public string TitleCz { get; set; } = string.Empty;

        /// <summary>
        /// Obsah zprávy v angličtině ve formátu Markdown
        /// </summary>
        public string ContentEn { get; set; } = string.Empty;

        /// <summary>
        /// Obsah zprávy v češtině ve formátu Markdown
        /// </summary>
        public string ContentCz { get; set; } = string.Empty;

        /// <summary>
        /// Shrnutí zprávy v angličtině
        /// </summary>
        public string SummaryEn { get; set; } = string.Empty;

        /// <summary>
        /// Shrnutí zprávy v češtině
        /// </summary>
        public string SummaryCz { get; set; } = string.Empty;

        /// <summary>
        /// URL, odkud byla zpráva stažena
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// URL obrázku k článku
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Název zdrojového webu
        /// </summary>
        public string SourceName { get; set; } = string.Empty;
        
        /// <summary>
        /// Datum a čas publikování zprávy
        /// </summary>
        public DateTime? PublishedDate { get; set; }

        /// <summary>
        /// Datum a čas importu zprávy
        /// </summary>
        public DateTime ImportedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Zda je zpráva aktivní a má se zobrazovat
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
} 