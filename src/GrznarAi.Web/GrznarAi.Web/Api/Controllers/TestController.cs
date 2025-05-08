using GrznarAi.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GrznarAi.Web.Api.Controllers
{
    [ApiController]
    [Route("api/testtimer")]
    public class TestController : ControllerBase
    {
        private readonly IWeatherHistoryService _weatherHistoryService;

        public TestController(IWeatherHistoryService weatherHistoryService)
        {
            _weatherHistoryService = weatherHistoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                await _weatherHistoryService.FetchAndStoreEcowittDataAsync();
                return Ok("Data fetched and stored successfully.");
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
