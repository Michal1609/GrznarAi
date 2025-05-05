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
    }
} 