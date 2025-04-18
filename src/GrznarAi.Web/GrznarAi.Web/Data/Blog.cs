using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Data
{
    public class Blog
    {
        public int Id { get; set; }

        // Časové značky pro sledování
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Počítadla hlasů
        public int LikesCount { get; set; } = 0;
        public int DislikesCount { get; set; } = 0;
        
        // Počítadlo komentářů
        public int CommentsCount { get; set; } = 0;

        // Navigační vlastnosti
        public ICollection<BlogContent> Contents { get; set; } = new List<BlogContent>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<BlogVote> Votes { get; set; } = new List<BlogVote>();
    }
} 