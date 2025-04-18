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

        // Navigační vlastnost pro vícejazyčný obsah
        public ICollection<BlogContent> Contents { get; set; } = new List<BlogContent>();
    }
} 