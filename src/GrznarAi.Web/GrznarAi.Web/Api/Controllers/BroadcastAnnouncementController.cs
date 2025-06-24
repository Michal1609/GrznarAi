using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrznarAi.Web.Data;
using GrznarAi.Web.Api.Models;
using Microsoft.AspNetCore.Authorization;
using GrznarAi.Web.Services;

namespace GrznarAi.Web.Api.Controllers
{
    /// <summary>
    /// API controller for managing broadcast announcements
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BroadcastAnnouncementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BroadcastAnnouncementController> _logger;
        private readonly IGlobalSettingsService _globalSettingsService;
        private readonly IBroadcastAnnouncementService _announcementService;

        public BroadcastAnnouncementController(
            ApplicationDbContext context,
            ILogger<BroadcastAnnouncementController> logger,
            IGlobalSettingsService globalSettingsService,
            IBroadcastAnnouncementService announcementService)
        {
            _context = context;
            _logger = logger;
            _globalSettingsService = globalSettingsService;
            _announcementService = announcementService;
        }

        /// <summary>
        /// Get paged list of broadcast announcements
        /// </summary>
        /// <param name="page">Page number (starts from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="search">Search term (fulltext)</param>
        /// <param name="day">Day to filter announcements (date only)</param>
        /// <returns>Paged list of announcements</returns>
        [HttpGet]
        public async Task<ActionResult<PagedBroadcastAnnouncementResponse>> GetAnnouncements(
            int page = 1, 
            int? pageSize = null,
            string? search = null,
            DateTime? day = null)
        {
            try
            {
                var result = await _announcementService.GetPagedAnnouncementsAsync(page, pageSize, search, day);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving broadcast announcements");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a specific broadcast announcement by ID
        /// </summary>
        /// <param name="id">Announcement ID</param>
        /// <returns>Announcement details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<BroadcastAnnouncementResponse>> GetAnnouncement(int id)
        {
            try
            {
                var announcement = await _context.BroadcastAnnouncements
                    .Where(ba => ba.Id == id && ba.IsActive)
                    .Select(ba => new BroadcastAnnouncementResponse
                    {
                        Id = ba.Id,
                        Content = ba.Content,
                        BroadcastDateTime = ba.BroadcastDateTime,
                        ImportedDateTime = ba.ImportedDateTime,
                        IsActive = ba.IsActive,
                        AudioUrl = ba.AudioUrl
                    })
                    .FirstOrDefaultAsync();

                if (announcement == null)
                {
                    return NotFound();
                }

                return Ok(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving broadcast announcement with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new broadcast announcement
        /// </summary>
        /// <param name="request">Announcement data</param>
        /// <returns>Created announcement</returns>
        [HttpPost]
        public async Task<ActionResult<BroadcastAnnouncementResponse>> CreateAnnouncement(
            [FromBody] CreateBroadcastAnnouncementRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var announcement = new BroadcastAnnouncement
                {
                    Content = request.Content.Trim(),
                    BroadcastDateTime = request.BroadcastDateTime,
                    ImportedDateTime = DateTime.UtcNow,
                    IsActive = true,
                    AudioUrl = request.AudioUrl
                };

                _context.BroadcastAnnouncements.Add(announcement);
                await _context.SaveChangesAsync();

                var response = new BroadcastAnnouncementResponse
                {
                    Id = announcement.Id,
                    Content = announcement.Content,
                    BroadcastDateTime = announcement.BroadcastDateTime,
                    ImportedDateTime = announcement.ImportedDateTime,
                    IsActive = announcement.IsActive,
                    AudioUrl = announcement.AudioUrl
                };

                _logger.LogInformation("Created new broadcast announcement with ID {Id}", announcement.Id);

                return CreatedAtAction(
                    nameof(GetAnnouncement),
                    new { id = announcement.Id },
                    response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating broadcast announcement");
                return StatusCode(500, "Internal server error");
            }
        }     
      
    }
} 