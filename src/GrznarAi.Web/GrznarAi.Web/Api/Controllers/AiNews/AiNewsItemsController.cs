using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GrznarAi.Web.Services;
using GrznarAi.Web.Api.Models.AiNews;
using GrznarAi.Web.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GrznarAi.Web.Api.Controllers.AiNews
{
    [ApiController]
    [Route("api/ainews/items")]
    public class AiNewsItemsController : ControllerBase
    {
        private readonly IAiNewsService _newsService;
        private readonly ITwitterService _twitterService;
        private readonly IAiNewsSourceService _sourceService;
        private readonly ILogger<AiNewsItemsController> _logger;

        public AiNewsItemsController(
            IAiNewsService newsService,
            ITwitterService twitterService,
            IAiNewsSourceService sourceService,
            ILogger<AiNewsItemsController> logger)
        {
            _newsService = newsService;
            _twitterService = twitterService;
            _sourceService = sourceService;
            _logger = logger;
        }       

        /// <summary>
        /// Přidá nové AI novinky
        /// </summary>
        /// <param name="request">Seznam novinek k přidání</param>
        /// <returns>Výsledek operace</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Dictionary<string, int>), 200)]
        public async Task<IActionResult> AddNewsItems([FromBody] List<AiNewsItemRequest> request)
        {
            if (request == null || request.Count == 0)
            {
                return BadRequest(new { message = "Nebyl poskytnut žádný seznam novinek" });
            }

            _logger.LogInformation("Přidávání {Count} AI novinek přes API", request.Count);
            
            var newsItems = new List<AiNewsItem>();
            // Slovník pro uložení názvů zdrojů, které budeme potřebovat aktualizovat
            var sourcesToUpdate = new HashSet<string>();
            
            foreach (var item in request)
            {
                Uri.TryCreate(item.ImageUrl, UriKind.Absolute, out var imageUrl);
                var newsItem = new AiNewsItem
                {
                    TitleEn = item.TitleEn,
                    TitleCz = item.TitleCz,
                    ContentEn = item.ContentEn ?? string.Empty,
                    ContentCz = item.ContentCz ?? string.Empty,
                    SummaryEn = item.SummaryEn,
                    SummaryCz = item.SummaryCz,
                    Url = item.Url,
                    ImageUrl = imageUrl?.ToString(),
                    SourceName = item.SourceName,
                    PublishedDate = item.PublishedDate,
                    ImportedDate = DateTime.UtcNow,
                    IsActive = true
                };
                
                newsItems.Add(newsItem);
                
                // Přidáme název zdroje do seznamu pro aktualizaci, pokud není prázdný
                if (!string.IsNullOrEmpty(item.SourceName))
                {
                    sourcesToUpdate.Add(item.SourceName);
                }
            }
            
            // Původní počet položek k uložení
            int requestedCount = newsItems.Count;
            
            // Uložení unikátních položek
            int addedCount = await _newsService.AddAiNewsItemsAsync(newsItems);
            
            // Počet přeskočených duplicitních položek
            int skippedCount = requestedCount - addedCount;
            
            _logger.LogInformation("Přidáno {AddedCount} AI novinek, přeskočeno {SkippedCount} duplicitních položek", 
                addedCount, skippedCount);
            
            // Aktualizujeme datum posledního stažení pro všechny zdroje
            if (sourcesToUpdate.Count > 0)
            {
                try
                {
                    // Použijeme službu pro hromadnou aktualizaci zdrojů
                    int updatedSources = await _sourceService.UpdateLastFetchedBulkAsync(sourcesToUpdate);
                    _logger.LogInformation("Aktualizováno LastFetched pro {Count} zdrojů", updatedSources);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Došlo k chybě při aktualizaci LastFetched v tabulce AiNewsSources");
                }
            }
            
            // Pokud byly přidány nějaké novinky, pošleme tweet
            if (addedCount > 0)
            {
                try
                {
                    // Odešleme tweet o nových AI novinkách
                    var tweetResult = await _twitterService.PostNewAiNewsAnnouncementAsync(addedCount);
                    
                    if (tweetResult)
                    {
                        _logger.LogInformation("Tweet o nových AI novinkách úspěšně odeslán");
                    }
                    else
                    {
                        _logger.LogWarning("Nepodařilo se odeslat tweet o nových AI novinkách");
                    }
                }
                catch (Exception ex)
                {
                    // Nechceme, aby chyba při odesílání tweetu ovlivnila výsledek API volání
                    _logger.LogError(ex, "Chyba při odesílání tweetu o nových AI novinkách");
                }
            }
            
            return Ok(new { 
                itemsRequested = requestedCount,
                itemsAdded = addedCount, 
                itemsSkipped = skippedCount 
            });
        }
    }
} 