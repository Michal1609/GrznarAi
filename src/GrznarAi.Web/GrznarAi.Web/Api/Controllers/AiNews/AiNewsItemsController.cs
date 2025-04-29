using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AiNewsItemsController> _logger;

        public AiNewsItemsController(
            IAiNewsService newsService,
            ILogger<AiNewsItemsController> logger)
        {
            _newsService = newsService;
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
            
            foreach (var item in request)
            {
                var newsItem = new AiNewsItem
                {
                    TitleEn = item.TitleEn,
                    TitleCz = item.TitleCz,
                    ContentEn = item.ContentEn ?? string.Empty,
                    ContentCz = item.ContentCz ?? string.Empty,
                    SummaryEn = item.SummaryEn,
                    SummaryCz = item.SummaryCz,
                    Url = item.Url,
                    ImageUrl = item.ImageUrl,
                    SourceName = item.SourceName,
                    PublishedDate = item.PublishedDate,
                    ImportedDate = DateTime.UtcNow,
                    IsActive = true
                };
                
                newsItems.Add(newsItem);
            }
            
            // Původní počet položek k uložení
            int requestedCount = newsItems.Count;
            
            // Uložení unikátních položek
            int addedCount = await _newsService.AddAiNewsItemsAsync(newsItems);
            
            // Počet přeskočených duplicitních položek
            int skippedCount = requestedCount - addedCount;
            
            _logger.LogInformation("Přidáno {AddedCount} AI novinek, přeskočeno {SkippedCount} duplicitních položek", 
                addedCount, skippedCount);
            
            return Ok(new { 
                itemsRequested = requestedCount,
                itemsAdded = addedCount, 
                itemsSkipped = skippedCount 
            });
        }
    }
} 