using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrznarAi.Web.Data;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrznarAi.Web.Api.Middleware
{
    /// <summary>
    /// Middleware pro ověření API klíče v hlavičce požadavků
    /// </summary>
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyMiddleware> _logger;
        private const string API_KEY_HEADER_NAME = "X-Api-Key";

        public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            // Ověřujeme pouze požadavky směřující na API endpointy
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            // Získání API klíče z hlavičky
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                _logger.LogWarning("API požadavek bez API klíče: {Path}", context.Request.Path);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Chybí API klíč" });
                return;
            }

            var apiKeyValue = extractedApiKey.ToString();

            // Ověření API klíče proti databázi
            using var dbContext = await contextFactory.CreateDbContextAsync();
            var isValidApiKey = await dbContext.ApiKeys
                .AnyAsync(k => k.Value == apiKeyValue && k.IsActive && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));

            if (!isValidApiKey)
            {
                _logger.LogWarning("Neplatný API klíč: {ApiKey}", apiKeyValue);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Neplatný API klíč" });
                return;
            }

            await _next(context);
        }
    }
} 