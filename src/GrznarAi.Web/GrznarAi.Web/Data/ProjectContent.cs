using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    // Definuje kompozitní index pro zajištění, že každý projekt má jen jeden obsah pro každý jazyk
    [Index(nameof(ProjectId), nameof(LanguageCode), IsUnique = true)]
    public class ProjectContent
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(10)] // "cs", "en", "de", atd.
        public string LanguageCode { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        // Navigační vlastnost
        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null!;
    }
} 