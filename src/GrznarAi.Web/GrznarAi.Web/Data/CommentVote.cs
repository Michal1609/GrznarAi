using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    // Zajišťuje, že existuje pouze jeden záznam pro kombinaci komentáře a uživatele/cookie
    [Index(nameof(CommentId), nameof(ApplicationUserId), IsUnique = true)]
    [Index(nameof(CommentId), nameof(UserCookieId), IsUnique = true)]
    public class CommentVote
    {
        public int Id { get; set; }

        [Required]
        public int CommentId { get; set; }

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
        [ForeignKey(nameof(CommentId))]
        public Comment Comment { get; set; } = null!;

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? User { get; set; }
    }
} 