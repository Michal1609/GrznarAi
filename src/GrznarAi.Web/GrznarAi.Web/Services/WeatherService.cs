using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GrznarAi.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrznarAi.Web.Services
{
    public interface IWeatherService
    {
        /// <summary>
        /// Získá aktuální data z meteostanice (z cache nebo přímo z API)
        /// </summary>
        Task<WeatherData> GetWeatherDataAsync();
        
        /// <summary>
        /// Vynutí načtení nových dat z API a aktualizaci v cache
        /// </summary>
        Task<WeatherData> RefreshWeatherDataAsync();
    }

    public class WeatherService : IWeatherService
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly ICacheService _cacheService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly string _cacheKey = "WeatherData";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public WeatherService(
            ILogger<WeatherService> logger,
            ICacheService cacheService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _cacheService = cacheService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<WeatherData> GetWeatherDataAsync()
        {
            return await _cacheService.GetOrCreateAsync(_cacheKey, FetchWeatherDataAsync, _cacheExpiration);
        }

        public async Task<WeatherData> RefreshWeatherDataAsync()
        {
            // Načíst nová data
            var weatherData = await FetchWeatherDataAsync();
            
            // Aktualizovat cache
            if (weatherData != null)
            {
                await _cacheService.SetAsync(_cacheKey, weatherData, _cacheExpiration);
            }
            
            return weatherData;
        }

        private async Task<WeatherData> FetchWeatherDataAsync()
        {
            try
            {
                // Získat nastavení z secrets nebo konfigurace
                string applicationKey = _configuration["WeatherService:ApplicationKey"] ?? "";
                string apiKey = _configuration["WeatherService:ApiKey"] ?? "";
                string mac = _configuration["WeatherService:Mac"] ?? "EC:FA:BC:71:F7:3B";
                
                if (string.IsNullOrEmpty(applicationKey) || string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Chybí API klíče pro WeatherService. Zkontrolujte konfiguraci.");
                    return null;
                }
                
                // Sestavit URL s klíči a parametry
                string url = $"https://api.ecowitt.net/api/v3/device/real_time" +
                             $"?application_key={applicationKey}" +
                             $"&api_key={apiKey}" +
                             $"&mac={mac}" +
                             $"&temp_unitid=1" +
                             $"&pressure_unitid=3" +
                             $"&wind_speed_unitid=6" +
                             $"&rainfall_unitid=12" +
                             $"&solar_irradiance_unitid=16" +
                             $"&capacity_unitid=24";
                
                // Volat API
                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Chyba při volání Weather API. StatusCode: {response.StatusCode}");
                    return null;
                }
                
                // Deserializovat odpověď
                var content = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<WeatherData>(content, _jsonOptions);
                
                if (weatherData?.Code != 0)
                {
                    _logger.LogError($"Weather API vrátilo chybu. Kód: {weatherData?.Code}, Zpráva: {weatherData?.Message}");
                    return null;
                }
                
                _logger.LogInformation("Data z meteostanice úspěšně načtena");
                return weatherData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při získávání dat z meteostanice");
                return null;
            }
        }
    }
} 