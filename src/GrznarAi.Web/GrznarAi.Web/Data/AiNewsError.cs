using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrznarAi.Web.Data
{
    /// <summary>
    /// Záznam o chybě při stahování AI novinek
    /// </summary>
    public class AiNewsError
    {
        /// <summary>
        /// Unikátní identifikátor chyby
        /// </summary>
        [Key]
        public int Id { get; set; }
             

        /// <summary>
        /// Datum a čas, kdy k chybě došlo
        /// </summary>
        [Required]
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Chybová zpráva
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Stack trace chyby
        /// </summary>
        public string? StackTrace { get; set; }
        
        /// <summary>
        /// ID zdroje, u kterého chyba nastala (pokud je známo)
        /// </summary>
        public int? SourceId { get; set; }
        
        /// <summary>
        /// Navigační vlastnost na zdroj
        /// </summary>
        [ForeignKey("SourceId")]
        public virtual AiNewsSource? Source { get; set; }
        
        /// <summary>
        /// Detaily chyby (například další kontext)
        /// </summary>
        [StringLength(2000)]
        public string? Details { get; set; }
        
        /// <summary>
        /// Kategorie chyby (např. "HTTP", "Parser", "Database")
        /// </summary>
        [StringLength(50)]
        public string? Category { get; set; }
        
        /// <summary>
        /// Příznak, zda byla chyba vyřešena
        /// </summary>
        public bool IsResolved { get; set; } = false;
        
        /// <summary>
        /// Datum a čas vyřešení chyby
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
        
        /// <summary>
        /// Popis řešení chyby
        /// </summary>
        [StringLength(1000)]
        public string? Resolution { get; set; }
    }
} 