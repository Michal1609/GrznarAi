using GrznarAi.Web.Core.Utils;
using GrznarAi.Web.Data;
using GrznarAi.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services.Home
{
    /// <summary>
    /// Service implementation for retrieving home page data
    /// </summary>
    public class HomeService : IHomeService
    {
        private readonly IBlogService _blogService;
        private readonly IAiNewsService _aiNewsService;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public HomeService(
            IBlogService blogService,
            IAiNewsService aiNewsService,
            IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _blogService = blogService;
            _aiNewsService = aiNewsService;
            _contextFactory = contextFactory;
        }

        /// <inheritdoc/>
        public async Task<List<HomeNewsItem>> GetLatestNewsAsync(string languageCode)
        {
            var result = new List<HomeNewsItem>();

            // Get blog posts from the last 24 hours
            var since = DateTime.UtcNow.AddHours(-24);
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var blogPosts = await context.BlogContents
                .Include(bc => bc.Blog)
                .Where(bc => bc.LanguageCode == languageCode && 
                       bc.IsPublished && 
                       bc.CreatedAt >= since)
                .ToListAsync();

            // Transform blog posts to HomeNewsItems
            foreach (var post in blogPosts)
            {
                string slug = SlugGenerator.GenerateSlug(post.Title);
                
                result.Add(new HomeNewsItem
                {
                    Id = post.Id.ToString(),
                    Title = post.Title,
                    Description = post.Description ?? string.Empty,
                    PublishedDate = post.CreatedAt,
                    Url = $"/blog/{slug}",
                    ImageUrl = null, // Blog posts don't have images currently
                    Source = "Blog",
                    SourceUrl = "/blog",
                    ItemType = HomeNewsItemType.Blog
                });
            }

            // Get AI news from the last 24 hours
            var aiNews = await context.AiNewsItems
                .Where(n => n.PublishedDate >= since)
                .ToListAsync();

            // Transform AI news to HomeNewsItems
            foreach (var news in aiNews)
            {
                result.Add(new HomeNewsItem
                {
                    Id = news.Id.ToString(),
                    Title = news.TitleCz,
                    Description = news.SummaryCz,
                    PublishedDate = news.PublishedDate ?? DateTime.UtcNow,
                    Url = $"/ai-news/detail/{news.Id}",
                    ImageUrl = news.ImageUrl,
                    Source = news.SourceName,
                    SourceUrl = news.Url,
                    ItemType = HomeNewsItemType.AiNews
                });
            }

            // Sort combined results by published date descending
            return result.OrderByDescending(item => item.PublishedDate).ToList();
        }
    }
} 