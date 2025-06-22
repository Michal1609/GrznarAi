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

        public BroadcastAnnouncementController(
            ApplicationDbContext context,
            ILogger<BroadcastAnnouncementController> logger,
            IGlobalSettingsService globalSettingsService)
        {
            _context = context;
            _logger = logger;
            _globalSettingsService = globalSettingsService;
        }

        /// <summary>
        /// Get paged list of broadcast announcements
        /// </summary>
        /// <param name="page">Page number (starts from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paged list of announcements</returns>
        [HttpGet]
        public async Task<ActionResult<PagedBroadcastAnnouncementResponse>> GetAnnouncements(
            int page = 1, 
            int? pageSize = null)
        {
            try
            {
                // Get page size from global settings if not provided
                var actualPageSize = pageSize ?? _globalSettingsService.GetInt("BroadcastAnnouncements.PageSize", 10);
                
                // Ensure minimum values
                page = Math.Max(1, page);
                actualPageSize = Math.Max(1, Math.Min(100, actualPageSize)); // Max 100 items per page

                var query = _context.BroadcastAnnouncements
                    .Where(ba => ba.IsActive)
                    .OrderByDescending(ba => ba.BroadcastDateTime);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / actualPageSize);

                var announcements = await query
                    .Skip((page - 1) * actualPageSize)
                    .Take(actualPageSize)
                    .Select(ba => new BroadcastAnnouncementResponse
                    {
                        Id = ba.Id,
                        Content = ba.Content,
                        BroadcastDateTime = ba.BroadcastDateTime,
                        ImportedDateTime = ba.ImportedDateTime,
                        IsActive = ba.IsActive,
                        AudioUrl = ba.AudioUrl
                    })
                    .ToListAsync();

                return Ok(new PagedBroadcastAnnouncementResponse
                {
                    Announcements = announcements,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = actualPageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                });
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