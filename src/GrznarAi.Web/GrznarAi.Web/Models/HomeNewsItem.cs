using System;

namespace GrznarAi.Web.Models
{
    /// <summary>
    /// Represents a news item displayed on the home page
    /// </summary>
    public class HomeNewsItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Source { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public HomeNewsItemType ItemType { get; set; }
    }

    /// <summary>
    /// Defines the type of news item
    /// </summary>
    public enum HomeNewsItemType
    {
        Blog,
        AiNews
    }
} 