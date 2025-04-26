using GrznarAi.Web.Data;
using System;
using System.Collections.Generic;

namespace GrznarAi.Web.Models
{
    public class AiNewsViewModel
    {
        /// <summary>
        /// Seznam AI novinek na aktuální stránce
        /// </summary>
        public List<AiNewsItem> Items { get; set; } = new List<AiNewsItem>();
        
        /// <summary>
        /// Celkový počet novinek
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Aktuální stránka (začíná od 1)
        /// </summary>
        public int CurrentPage { get; set; } = 1;
        
        /// <summary>
        /// Počet položek na stránku
        /// </summary>
        public int PageSize { get; set; } = 20;
        
        /// <summary>
        /// Vyhledávaný text
        /// </summary>
        public string SearchTerm { get; set; } = string.Empty;
        
        /// <summary>
        /// Filtrování podle roku
        /// </summary>
        public int? Year { get; set; }
        
        /// <summary>
        /// Filtrování podle měsíce
        /// </summary>
        public int? Month { get; set; }
        
        /// <summary>
        /// Seznam dostupných archivních měsíců
        /// </summary>
        public List<(int Year, int Month, int Count)> ArchiveMonths { get; set; } = new List<(int Year, int Month, int Count)>();
        
        /// <summary>
        /// Celkový počet stránek
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        /// <summary>
        /// Zda existuje předchozí stránka
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;
        
        /// <summary>
        /// Zda existuje následující stránka
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Zda je aktivní filtrování podle data
        /// </summary>
        public bool IsArchiveFiltering => Year.HasValue;
    }
} 