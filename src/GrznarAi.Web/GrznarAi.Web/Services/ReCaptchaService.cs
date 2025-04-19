using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface IReCaptchaService
    {
        Task<bool> VerifyAsync(string token);
        string GetSiteKey();
    }

    public class ReCaptchaService : IReCaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReCaptchaService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public string GetSiteKey()
        {
            return _configuration["ReCaptcha:SiteKey"] ?? "test-key";
        }

        public async Task<bool> VerifyAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var secretKey = _configuration["ReCaptcha:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                return true; // Pro testovací účely vždy vrátíme true, pokud není konfigurován secret key
                
            var minimumScore = double.Parse(_configuration["ReCaptcha:MinimumScore"] ?? "0.5");
            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                null);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<ReCaptchaResponse>();
            return result != null && result.Success && result.Score >= minimumScore;
        }

        private class ReCaptchaResponse
        {
            public bool Success { get; set; }
            public double Score { get; set; }
            public string? Action { get; set; }
            public string? Hostname { get; set; }
        }
    }
} 