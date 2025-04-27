using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GrznarAi.Web.Services;
using GrznarAi.Web.Api.Models.AiNews;
using System.Threading.Tasks;
using System.Linq;

namespace GrznarAi.Web.Api.Controllers.AiNews
{
    [ApiController]
    [Route("api/ainews/sources")]
    public class AiNewsSourcesController : ControllerBase
    {
        private readonly IAiNewsSourceService _sourceService;
        private readonly ILogger<AiNewsSourcesController> _logger;

        public AiNewsSourcesController(
            IAiNewsSourceService sourceService,
            ILogger<AiNewsSourcesController> logger)
        {
            _sourceService = sourceService;
            _logger = logger;
        }

        /// <summary>
        /// Získá seznam aktivních zdrojů AI novinek
        /// </summary>
        /// <returns>Seznam aktivních zdrojů</returns>
        [HttpGet]
        [ProducesResponseType(typeof(AiNewsSourceListResponse), 200)]
        public async Task<IActionResult> GetSources()
        {
            _logger.LogInformation("Získávání seznamu aktivních zdrojů AI novinek");
            
            var sources = await _sourceService.GetActiveSourcesAsync();
            
            var response = new AiNewsSourceListResponse
            {
                Sources = sources.Select(AiNewsSourceResponse.FromSource).ToList(),
                TotalCount = sources.Count
            };
            
            return Ok(response);
        }
    }
} 