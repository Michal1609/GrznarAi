using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GrznarAi.Web.Data;
using GrznarAi.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Tools
{
    public class SitemapGenerator
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IBlogService _blogService;
        private readonly IAiNewsService _aiNewsService;
        private readonly IProjectService _projectService;
        private readonly string _baseUrl;

        public SitemapGenerator(
            IDbContextFactory<ApplicationDbContext> contextFactory, 
            IBlogService blogService,
            IAiNewsService aiNewsService,
            IProjectService projectService,
            string baseUrl)
        {
            _contextFactory = contextFactory;
            _blogService = blogService;
            _aiNewsService = aiNewsService;
            _projectService = projectService;
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<string> GenerateSitemapAsync()
        {
            // Create XML document with required namespaces
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement root = new XElement(xmlns + "urlset");
            XDocument document = new XDocument(root);

            // Add static pages
            AddUrl(root, xmlns, "/", 1.0, "daily");
            AddUrl(root, xmlns, "/projects", 0.8, "weekly");
            AddUrl(root, xmlns, "/blog", 0.8, "daily");
            AddUrl(root, xmlns, "/ainews", 0.8, "daily");
            AddUrl(root, xmlns, "/meteo", 0.8, "daily");
            AddUrl(root, xmlns, "/contact", 0.5, "monthly");

            // Add dynamic pages
            await AddBlogPostsAsync(root, xmlns);
            await AddAiNewsItemsAsync(root, xmlns);
            await AddProjectDetailsAsync(root, xmlns);

            // Return the XML as a string
            return document.Declaration == null 
                ? document.ToString() 
                : document.Declaration.ToString() + Environment.NewLine + document.ToString();
        }

        private void AddUrl(XElement root, XNamespace xmlns, string url, double priority, string changeFreq)
        {
            var fullUrl = _baseUrl + url;
            
            var element = new XElement(xmlns + "url",
                new XElement(xmlns + "loc", fullUrl),
                new XElement(xmlns + "lastmod", DateTime.UtcNow.ToString("yyyy-MM-dd")),
                new XElement(xmlns + "changefreq", changeFreq),
                new XElement(xmlns + "priority", priority.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture))
            );

            root.Add(element);
        }

        private async Task AddBlogPostsAsync(XElement root, XNamespace xmlns)
        {
            try
            {
                // Get blog posts for English language (primary)
                var posts = await _blogService.GetPublishedBlogsAsync("en", 0, 1000);
                
                foreach (var post in posts)
                {
                    if (!string.IsNullOrEmpty(post.Title))
                    {
                        var slug = GenerateSlug(post.Title);
                        AddUrl(root, xmlns, $"/blog/post/{slug}", 0.7, "weekly");
                    }
                }
                
                // Also get Czech posts that might be different
                var czPosts = await _blogService.GetPublishedBlogsAsync("cs", 0, 1000);
                
                foreach (var post in czPosts)
                {
                    // Only add if not already added and has a valid title
                    if (!string.IsNullOrEmpty(post.Title) && !posts.Any(p => p.BlogId == post.BlogId))
                    {
                        var slug = GenerateSlug(post.Title);
                        AddUrl(root, xmlns, $"/blog/post/{slug}", 0.7, "weekly");
                    }
                }
            }
            catch (Exception)
            {
                // Fail silently - if we can't get blog posts, just skip them in the sitemap
            }
        }

        private async Task AddAiNewsItemsAsync(XElement root, XNamespace xmlns)
        {
            try
            {
                var newsItems = await _aiNewsService.GetAiNewsAsync(1, 1000);
                
                foreach (var newsItem in newsItems.Items)
                {
                    AddUrl(root, xmlns, $"/ainews/{newsItem.Id}", 0.6, "weekly");
                }
            }
            catch (Exception)
            {
                // Fail silently
            }
        }

        private async Task AddProjectDetailsAsync(XElement root, XNamespace xmlns)
        {
            try
            {
                var projects = await _projectService.GetProjectsAsync();
                
                foreach (var project in projects)
                {
                    AddUrl(root, xmlns, $"/project/{project.Id}", 0.7, "monthly");
                }
            }
            catch (Exception)
            {
                // Fail silently
            }
        }
        
        // Pomocná metoda pro generování slugu z titulku - stejná jako v BlogService
        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return string.Empty;
            }

            // Jednoduchá implementace pro vytvoření slugu z titulku
            return title
                .ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("č", "c")
                .Replace("ř", "r")
                .Replace("ž", "z")
                .Replace("š", "s")
                .Replace("ě", "e")
                .Replace("ť", "t")
                .Replace("ď", "d")
                .Replace("ň", "n")
                .Replace("ú", "u")
                .Replace("ů", "u")
                .Replace("á", "a")
                .Replace("í", "i")
                .Replace("é", "e")
                .Replace("ý", "y")
                .Replace("ó", "o");
        }
    }
} 