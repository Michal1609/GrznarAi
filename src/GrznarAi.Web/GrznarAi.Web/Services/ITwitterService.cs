using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Service for posting messages on Twitter
    /// </summary>
    public interface ITwitterService
    {
        /// <summary>
        /// Post a tweet with text only
        /// </summary>
        /// <param name="message">Message to post</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> PostTweetAsync(string message);
        
        /// <summary>
        /// Post a tweet with text and image
        /// </summary>
        /// <param name="message">Message to post</param>
        /// <param name="imagePath">Path to image (relative or absolute)</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> PostTweetWithImageAsync(string message, string imagePath);
        
        /// <summary>
        /// Post a tweet about new AI news
        /// </summary>
        /// <param name="newItemsCount">Number of new news items</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> PostNewAiNewsAnnouncementAsync(int newItemsCount);
        
        /// <summary>
        /// Post a tweet about new blog post
        /// </summary>
        /// <param name="blogPostTitle">Blog post title</param>
        /// <param name="blogPostUrl">Blog post URL</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> PostNewBlogPostAnnouncementAsync(string blogPostTitle, string blogPostUrl);
    }
} 