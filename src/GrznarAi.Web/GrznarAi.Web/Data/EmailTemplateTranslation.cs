using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    // Definuji unikátní index pro kombinaci TemplateId a LanguageCode
    [Index(nameof(EmailTemplateId), nameof(LanguageCode), IsUnique = true)]
    public class EmailTemplateTranslation
    {
        public int Id { get; set; }

        [Required]
        public int EmailTemplateId { get; set; }

        [Required]
        [StringLength(10)] // např. "cs", "en", "de-CH"
        public string LanguageCode { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        // Navigační vlastnost pro šablonu
        [ForeignKey(nameof(EmailTemplateId))]
        public virtual EmailTemplate EmailTemplate { get; set; } = null!;
    }
} 