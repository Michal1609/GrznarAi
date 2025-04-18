using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    // Zajišťuje, že existuje pouze jeden záznam pro kombinaci blogu a uživatele/cookie
    [Index(nameof(BlogId), nameof(ApplicationUserId), IsUnique = true)]
    [Index(nameof(BlogId), nameof(UserCookieId), IsUnique = true)]
    public class BlogVote
    {
        public int Id { get; set; }

        [Required]
        public int BlogId { get; set; }

        // Odkaz na přihlášeného uživatele, pokud je k dispozici
        public string? ApplicationUserId { get; set; }

        // Pro identifikaci nepřihlášených uživatelů
        [StringLength(100)]
        public string? UserCookieId { get; set; }

        // Typ hlasu: true = like, false = dislike
        public bool IsLike { get; set; }

        // Časové údaje
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigační vlastnosti
        [ForeignKey(nameof(BlogId))]
        public Blog Blog { get; set; } = null!;

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? User { get; set; }
    }
} 