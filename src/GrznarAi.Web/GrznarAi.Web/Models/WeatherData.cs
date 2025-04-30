using System.Text.Json.Serialization;
using System.Text.Json;

namespace GrznarAi.Web.Models
{
    public class WeatherData
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("data")]
        public WeatherDataInfo Data { get; set; }
    }

    public class WeatherDataInfo
    {
        [JsonPropertyName("outdoor")]
        public OutdoorData Outdoor { get; set; }

        [JsonPropertyName("indoor")]
        public IndoorData Indoor { get; set; }

        [JsonPropertyName("solar_and_uvi")]
        public SolarAndUviData SolarAndUvi { get; set; }

        [JsonPropertyName("rainfall")]
        public RainfallDataExtended Rainfall { get; set; }

        [JsonPropertyName("wind")]
        public WindDataExtended Wind { get; set; }

        [JsonPropertyName("pressure")]
        public PressureDataExtended Pressure { get; set; }
    }

    public class OutdoorData
    {
        [JsonPropertyName("temperature")]
        public TemperatureData Temperature { get; set; }

        [JsonPropertyName("humidity")]
        public HumidityData Humidity { get; set; }

        [JsonPropertyName("feels_like")]
        public FeelsLikeData FeelsLike { get; set; }

        [JsonPropertyName("app_temp")]
        public AppTempData AppTemp { get; set; }

        [JsonPropertyName("dew_point")]
        public DewPointData DewPoint { get; set; }
    }

    public class IndoorData
    {
        [JsonPropertyName("temperature")]
        public TemperatureData Temperature { get; set; }

        [JsonPropertyName("humidity")]
        public HumidityData Humidity { get; set; }
    }

    public class SolarAndUviData
    {
        [JsonPropertyName("solar")]
        public SolarRadiationData Solar { get; set; }

        [JsonPropertyName("uvi")]
        public UviData Uvi { get; set; }
    }

    public class WindDataExtended
    {
        [JsonPropertyName("wind_speed")]
        public WindSpeedData WindSpeed { get; set; }

        [JsonPropertyName("wind_gust")]
        public WindGustData WindGust { get; set; }

        [JsonPropertyName("wind_direction")]
        public WindDirectionData WindDirection { get; set; }
    }

    public class PressureDataExtended
    {
        [JsonPropertyName("relative")]
        public PressureData Relative { get; set; }

        [JsonPropertyName("absolute")]
        public PressureData Absolute { get; set; }
    }

    public class RainfallDataExtended
    {
        [JsonPropertyName("rain_rate")]
        public RainRateData RainRate { get; set; }

        [JsonPropertyName("daily")]
        public RainDailyData Daily { get; set; }

        [JsonPropertyName("weekly")]
        public RainWeeklyData Weekly { get; set; }

        [JsonPropertyName("monthly")]
        public RainMonthlyData Monthly { get; set; }

        [JsonPropertyName("yearly")]
        public RainYearlyData Yearly { get; set; }

        [JsonPropertyName("event")]
        public RainEventData Event { get; set; }

        [JsonPropertyName("hourly")]
        public RainHourlyData Hourly { get; set; }
    }

    // Základní třída pro hodnoty s časem
    public class BaseTimeData
    {
        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }
    }

    // Třída pro práci s hodnotami jako string, které budou konvertovány na double
    public class BaseValueData : BaseTimeData
    {
        private string _rawValue;
        private double? _parsedValue;

        [JsonPropertyName("value")]
        public string RawValue
        {
            get => _rawValue;
            set
            {
                _rawValue = value;
                if (double.TryParse(value, out double parsed))
                {
                    _parsedValue = parsed;
                }
                else
                {
                    _parsedValue = null;
                }
            }
        }

        [JsonIgnore]
        public double? Value => _parsedValue;

        [JsonIgnore]
        public bool HasValue => Value.HasValue;
    }

    public class TemperatureData : BaseValueData { }
    public class HumidityData : BaseValueData { }
    public class FeelsLikeData : BaseValueData { }
    public class AppTempData : BaseValueData { }
    public class DewPointData : BaseValueData { }
    public class WindSpeedData : BaseValueData { }
    public class WindDirectionData : BaseValueData { }
    public class WindGustData : BaseValueData { }
    public class UviData : BaseValueData { }
    public class SolarRadiationData : BaseValueData { }
    public class RainRateData : BaseValueData { }
    public class RainDailyData : BaseValueData { }
    public class RainWeeklyData : BaseValueData { }
    public class RainMonthlyData : BaseValueData { }
    public class RainYearlyData : BaseValueData { }
    public class RainEventData : BaseValueData { }
    public class RainHourlyData : BaseValueData { }
    public class PressureData : BaseValueData { }
} 