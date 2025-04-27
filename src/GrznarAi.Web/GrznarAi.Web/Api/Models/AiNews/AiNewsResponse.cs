using GrznarAi.Web.Data;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GrznarAi.Web.Api.Models.AiNews
{
    /// <summary>
    /// Odpověď pro API endpoint získání zdrojů AI novinek
    /// </summary>
    public class AiNewsSourceResponse
    {
        /// <summary>
        /// ID zdroje
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Název zdroje
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// URL zdroje
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Typ zdroje (RSS, Web, API, etc.)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SourceType Type { get; set; }

        /// <summary>
        /// Zda je zdroj aktivní
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Datum posledního stažení
        /// </summary>
        public DateTime? LastFetched { get; set; }

        /// <summary>
        /// Dodatečné parametry pro stažení (JSON)
        /// </summary>
        public string? Parameters { get; set; }

        /// <summary>
        /// Vytvoří odpověď ze zdroje
        /// </summary>
        /// <param name="source">Zdroj</param>
        /// <returns>Odpověď</returns>
        public static AiNewsSourceResponse FromSource(AiNewsSource source)
        {
            return new AiNewsSourceResponse
            {
                Id = source.Id,
                Name = source.Name,
                Url = source.Url,
                Type = source.Type,
                IsActive = source.IsActive,
                LastFetched = source.LastFetched,
                Parameters = source.Parameters
            };
        }
    }

    /// <summary>
    /// Seznam zdrojů AI novinek
    /// </summary>
    public class AiNewsSourceListResponse
    {
        /// <summary>
        /// Seznam zdrojů
        /// </summary>
        public List<AiNewsSourceResponse> Sources { get; set; } = new();

        /// <summary>
        /// Celkový počet zdrojů
        /// </summary>
        public int TotalCount { get; set; }
    }
} 