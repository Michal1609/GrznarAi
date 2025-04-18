using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrznarAi.Web.Data
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AuthorName { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        public string? AuthorEmail { get; set; }

        // Odkaz na ID přihlášeného uživatele, pokud je k dispozici
        public string? ApplicationUserId { get; set; }

        // Pro identifikaci uživatelů, kteří nejsou přihlášení, ale vložili komentář
        [StringLength(100)]
        public string? UserCookieId { get; set; }

        [Required]
        public int BlogId { get; set; }

        // Hierarchická struktura - ID nadřazeného komentáře
        public int? ParentCommentId { get; set; }

        // Časové údaje
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Další stavy
        public bool IsApproved { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        // Počítadla hlasů
        public int LikesCount { get; set; } = 0;
        public int DislikesCount { get; set; } = 0;

        // Navigační vlastnosti
        [ForeignKey(nameof(BlogId))]
        public Blog Blog { get; set; } = null!;

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? User { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public Comment? ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();
    }
} 