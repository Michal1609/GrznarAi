using System;

namespace GrznarAi.Web.Components.Pages.Blog.Models
{
    /// <summary>
    /// Model pro zobrazení archivních měsíců v blogu
    /// </summary>
    public class ArchiveMonthViewModel
    {
        /// <summary>
        /// Rok archivního záznamu
        /// </summary>
        public int Year { get; set; }
        
        /// <summary>
        /// Měsíc archivního záznamu (1-12)
        /// </summary>
        public int Month { get; set; }
        
        /// <summary>
        /// Počet příspěvků v daném měsíci
        /// </summary>
        public int Count { get; set; }
        
        /// <summary>
        /// Formátovaný název měsíce (např. "Leden 2023")
        /// </summary>
        public string DisplayName => $"{GetMonthName(Month)} {Year}";
        
        /// <summary>
        /// Převede číslo měsíce na jeho textovou reprezentaci v češtině
        /// </summary>
        private string GetMonthName(int month) => month switch
        {
            1 => "Leden",
            2 => "Únor",
            3 => "Březen",
            4 => "Duben",
            5 => "Květen",
            6 => "Červen",
            7 => "Červenec",
            8 => "Srpen",
            9 => "Září",
            10 => "Říjen",
            11 => "Listopad",
            12 => "Prosinec",
            _ => throw new ArgumentOutOfRangeException(nameof(month), "Neplatný měsíc")
        };

        public string ToString(string format)
        {
            if (format == "MMMM yyyy")
                return DisplayName;
            
            return $"{Month}/{Year}";
        }
    }
} 