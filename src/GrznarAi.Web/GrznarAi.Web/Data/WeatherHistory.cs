using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    [Index(nameof(Date))]
    public class WeatherHistory
    {
        [Key]
        public int HistoryId { get; set; }

        public DateTime Date { get; set; }

        public float? TemperatureIn { get; set; }

        public float? TemperatureOut { get; set; }

        public float? Chill { get; set; }

        public float? DewIn { get; set; }

        public float? DewOut { get; set; }

        public float? HeatIn { get; set; }

        public float? Heat { get; set; }

        public float? HumidityIn { get; set; }

        public float? HumidityOut { get; set; }

        public float? WindSpeedHi { get; set; }

        public float? WindSpeedAvg { get; set; }

        public float? WindDirection { get; set; }

        public float? Bar { get; set; }

        public float? Rain { get; set; }

        public float? RainRate { get; set; }

        public float? SolarRad { get; set; }

        public int? Uvi { get; set; }
        
        // Dodatečné vlastnosti pro analýzu trendů
        [NotMapped]
        public float? AvgTemperature { get; set; }
        
        [NotMapped]
        public float? MaxTemperature { get; set; }
        
        // Dodatečné vlastnosti pro analýzu vlhkosti
        [NotMapped]
        public float? MinHumidity { get; set; }
        
        [NotMapped]
        public float? AvgHumidity { get; set; }
        
        [NotMapped]
        public float? MaxHumidity { get; set; }
        
        // Dodatečné vlastnosti pro analýzu tlaku
        [NotMapped]
        public float? MinBar { get; set; }
        
        [NotMapped]
        public float? AvgBar { get; set; }
        
        [NotMapped]
        public float? MaxBar { get; set; }
        
        // Dodatečné vlastnosti pro analýzu větru
        [NotMapped]
        public float? MinWindSpeed { get; set; }
        
        [NotMapped]
        public float? MaxWindSpeed { get; set; }
        
        [NotMapped]
        public float? WindGust { get; set; }
        
        // Dodatečné vlastnosti pro analýzu srážek
        [NotMapped]
        public float? TotalRain { get; set; }
        
        // Dodatečné vlastnosti pro analýzu slunečního záření
        [NotMapped]
        public float? MinSolarRad { get; set; }
        
        [NotMapped]
        public float? AvgSolarRad { get; set; }
        
        [NotMapped]
        public float? MaxSolarRad { get; set; }
        
        [NotMapped]
        public float? SunshineHours { get; set; }
        
        // Dodatečné vlastnosti pro analýzu UV indexu
        [NotMapped]
        public float? AvgUvi { get; set; }
        
        [NotMapped]
        public float? MaxUvi { get; set; }
    }
} 