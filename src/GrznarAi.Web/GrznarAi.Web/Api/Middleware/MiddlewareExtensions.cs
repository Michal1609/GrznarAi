using Microsoft.AspNetCore.Builder;

namespace GrznarAi.Web.Api.Middleware
{
    /// <summary>
    /// Extension metody pro registraci middleware
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Přidá middleware pro ověření API klíče
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
} 