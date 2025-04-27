using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GrznarAi.Web.Services;
using GrznarAi.Web.Api.Models.AiNews;
using GrznarAi.Web.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Api.Controllers.AiNews
{
    [ApiController]
    [Route("api/ainews/errors")]
    public class AiNewsErrorsController : ControllerBase
    {
        private readonly IAiNewsErrorService _errorService;
        private readonly ILogger<AiNewsErrorsController> _logger;

        public AiNewsErrorsController(
            IAiNewsErrorService errorService,
            ILogger<AiNewsErrorsController> logger)
        {
            _errorService = errorService;
            _logger = logger;
        }

        /// <summary>
        /// Přidá nové chyby při stahování AI novinek
        /// </summary>
        /// <param name="request">Seznam chyb k přidání</param>
        /// <returns>Výsledek operace</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Dictionary<string, int>), 200)]
        public async Task<IActionResult> AddErrors([FromBody] List<AiNewsErrorRequest> request)
        {
            if (request == null || request.Count == 0)
            {
                return BadRequest(new { message = "Nebyl poskytnut žádný seznam chyb" });
            }

            _logger.LogInformation("Přidávání {Count} chyb AI novinek přes API", request.Count);
            
            var errors = new List<AiNewsError>();
            
            foreach (var item in request)
            {
                var error = new AiNewsError
                {
                    Message = item.Message,
                    StackTrace = item.StackTrace,
                    SourceId = item.SourceId,
                    Details = item.Details,
                    Category = item.Category,
                    OccurredAt = DateTime.UtcNow,
                    IsResolved = false
                };
                
                errors.Add(error);
            }
            
            await _errorService.AddErrorsAsync(errors);
            
            return Ok(new { errorsAdded = errors.Count });
        }
    }
} 