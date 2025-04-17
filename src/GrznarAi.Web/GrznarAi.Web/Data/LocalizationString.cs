using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    // Define a composite unique index on Key and LanguageCode
    [Index(nameof(Key), nameof(LanguageCode), IsUnique = true)]
    public class LocalizationString
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)] // Key length limit
        public string Key { get; set; } = string.Empty;

        [Required]
        [StringLength(10)] // e.g., "cs", "en", "de-CH"
        public string LanguageCode { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        // Optional: Description for admin context (can describe the key's purpose)
        public string? Description { get; set; }
    }
} 