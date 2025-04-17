using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    [Index(nameof(Key), IsUnique = true)] // Ensure keys are unique
    public class LocalizationString
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)] // Key length limit
        public string Key { get; set; } = string.Empty;

        [Required]
        public string ValueCs { get; set; } = string.Empty;

        [Required]
        public string ValueEn { get; set; } = string.Empty;

        // Optional: Description for admin context
        public string? Description { get; set; }
    }
} 