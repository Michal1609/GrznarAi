using System;
using System.Collections.Generic;

namespace GrznarAi.Web.Models
{
    /// <summary>
    /// Model pro data trendů počasí s agregovanými hodnotami pro grafy
    /// </summary>
    public class WeatherTrendData
    {
        // Datum a čas měření
        public DateTime Date { get; set; }
        
        // Teplotní data
        public float? TemperatureMin { get; set; }
        public float? TemperatureAvg { get; set; }
        public float? TemperatureMax { get; set; }
        
        // Data o vlhkosti
        public float? HumidityMin { get; set; }
        public float? HumidityAvg { get; set; }
        public float? HumidityMax { get; set; }
        
        // Data o tlaku vzduchu
        public float? PressureMin { get; set; }
        public float? PressureAvg { get; set; }
        public float? PressureMax { get; set; }
        
        // Data o větru
        public float? WindSpeedMin { get; set; }
        public float? WindSpeedAvg { get; set; }
        public float? WindSpeedMax { get; set; }
        public float? WindGust { get; set; }
        
        // Data o srážkách
        public float? Rainfall { get; set; }
        public float? RainfallRate { get; set; }
        
        // Data o slunečním záření
        public float? SolarRadiationMin { get; set; }
        public float? SolarRadiationAvg { get; set; }
        public float? SolarRadiationMax { get; set; }
        public float? SunshineHours { get; set; }
        
        // UV index
        public int? UVIndex { get; set; }
    }
    
    /// <summary>
    /// Typový výčet pro období analýzy
    /// </summary>
    public enum TrendPeriodType
    {
        Day,
        Week,
        Month,
        Year
    }
} 