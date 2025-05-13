using GrznarAi.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Service for retrieving home page data
    /// </summary>
    public interface IHomeService
    {
        /// <summary>
        /// Gets combined list of blog posts and AI news from the last 24 hours
        /// </summary>
        /// <param name="languageCode">Language code for blog posts</param>
        /// <returns>Combined list of news items</returns>
        Task<List<HomeNewsItem>> GetLatestNewsAsync(string languageCode);
    }
} 