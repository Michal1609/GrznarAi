using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Data
{
    public class EmailTemplate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TemplateKey { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        // Seznam možných placeholderů pro tuto šablonu
        public string AvailablePlaceholders { get; set; } = string.Empty;

        // Navigační vlastnost pro jazykové verze
        public virtual List<EmailTemplateTranslation> Translations { get; set; } = new();
    }
} 