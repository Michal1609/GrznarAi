using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GrznarAi.Web.Models
{
    public class EcowittApiResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("data")]
        public EcowittData Data { get; set; }
    }

    public class EcowittData
    {
        [JsonPropertyName("outdoor")]
        public EcowittOutdoor Outdoor { get; set; }

        [JsonPropertyName("rainfall")]
        public EcowittRainfall Rainfall { get; set; }

        [JsonPropertyName("indoor")]
        public EcowittIndoor Indoor { get; set; }

        [JsonPropertyName("solar_and_uvi")]
        public EcowittSolarAndUvi SolarAndUvi { get; set; }

        [JsonPropertyName("wind")]
        public EcowittWind Wind { get; set; }

        [JsonPropertyName("pressure")]
        public EcowittPressure Pressure { get; set; }
    }

    public class EcowittOutdoor
    {
        [JsonPropertyName("temperature")]
        public EcowittValueWithDict Temperature { get; set; }

        [JsonPropertyName("feels_like")]
        public EcowittValueWithDict FeelsLike { get; set; }

        [JsonPropertyName("dew_point")]
        public EcowittValueWithDict DewPoint { get; set; }

        [JsonPropertyName("humidity")]
        public EcowittValueWithDict Humidity { get; set; }

        [JsonPropertyName("app_temp")]
        public EcowittValueWithDict AppTemp { get; set; }
    }

    public class EcowittRainfall
    {
        [JsonPropertyName("rain_rate")]
        public EcowittValueWithDict RainRate { get; set; }

        [JsonPropertyName("daily")]
        public EcowittValueWithDict Daily { get; set; }

        [JsonPropertyName("event")]
        public EcowittValueWithDict Event { get; set; }

        [JsonPropertyName("hourly")]
        public EcowittValueWithDict Hourly { get; set; }

        [JsonPropertyName("weekly")]
        public EcowittValueWithDict Weekly { get; set; }

        [JsonPropertyName("monthly")]
        public EcowittValueWithDict Monthly { get; set; }

        [JsonPropertyName("yearly")]
        public EcowittValueWithDict Yearly { get; set; }
    }

    public class EcowittIndoor
    {
        [JsonPropertyName("temperature")]
        public EcowittValueWithDict Temperature { get; set; }

        [JsonPropertyName("humidity")]
        public EcowittValueWithDict Humidity { get; set; }
    }

    public class EcowittSolarAndUvi
    {
        [JsonPropertyName("solar")]
        public EcowittValueWithDict Solar { get; set; }

        [JsonPropertyName("uvi")]
        public EcowittValueWithDict Uvi { get; set; }
    }

    public class EcowittWind
    {
        [JsonPropertyName("wind_speed")]
        public EcowittValueWithDict WindSpeed { get; set; }

        [JsonPropertyName("wind_gust")]
        public EcowittValueWithDict WindGust { get; set; }

        [JsonPropertyName("wind_direction")]
        public EcowittValueWithDict WindDirection { get; set; }
    }

    public class EcowittPressure
    {
        [JsonPropertyName("relative")]
        public EcowittValueWithDict Relative { get; set; }

        [JsonPropertyName("absolute")]
        public EcowittValueWithDict Absolute { get; set; }
    }

    public class EcowittValueWithDict
    {
        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("list")]
        public Dictionary<string, string> List { get; set; }
    }
} 