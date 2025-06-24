using GrznarAi.Web.Components.Pages.Blog.Models;
using GrznarAi.Web.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Service for managing blog posts and their content.
    /// </summary>
    public interface IBlogService
    {
        /// <summary>
        /// Retrieves all blog posts.
        /// </summary>
        /// <returns>A list of all blogs.</returns>
        Task<List<Blog>> GetBlogsAsync();

        /// <summary>
        /// Retrieves a specific blog post by its ID.
        /// </summary>
        /// <param name="blogId">The ID of the blog to retrieve.</param>
        /// <returns>The blog with the specified ID, or null if not found.</returns>
        Task<Blog?> GetBlogByIdAsync(int blogId);

        /// <summary>
        /// Creates a new, empty blog post.
        /// </summary>
        /// <returns>The newly created blog post.</returns>
        Task<Blog> CreateBlogAsync();

        /// <summary>
        /// Updates the image URL for a specific blog post.
        /// </summary>
        /// <param name="blogId">The ID of the blog to update.</param>
        /// <param name="imageUrl">The new image URL.</param>
        Task UpdateBlogImageUrlAsync(int blogId, string? imageUrl);

        /// <summary>
        /// Deletes a blog post and all its associated content.
        /// </summary>
        /// <param name="id">The ID of the blog to delete.</param>
        Task DeleteBlogAsync(int id);

        /// <summary>
        /// Retrieves the content for a specific blog in a specific language.
        /// </summary>
        /// <param name="blogId">The ID of the blog.</param>
        /// <param name="languageCode">The language code for the content.</param>
        /// <returns>The blog content, or null if not found.</returns>
        Task<BlogContent?> GetBlogContentAsync(int blogId, string languageCode);

        /// <summary>
        /// Retrieves all blog content for a specific language.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <returns>A list of blog content in the specified language.</returns>
        Task<List<BlogContent>> GetBlogContentsByLanguageAsync(string languageCode);

        /// <summary>
        /// Creates new blog content or updates existing content.
        /// </summary>
        /// <param name="blogContent">The blog content to create or update.</param>
        /// <returns>The created or updated blog content.</returns>
        Task<BlogContent> CreateOrUpdateBlogContentAsync(BlogContent blogContent);

        /// <summary>
        /// Deletes specific blog content by its ID.
        /// </summary>
        /// <param name="blogContentId">The ID of the blog content to delete.</param>
        Task DeleteBlogContentAsync(int blogContentId);

        /// <summary>
        /// Retrieves a paginated list of published blogs for a specific language.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <param name="take">The number of items to take.</param>
        /// <returns>A list of published blog content.</returns>
        Task<List<BlogContent>> GetPublishedBlogsAsync(string languageCode, int skip = 0, int take = 10);

        /// <summary>
        /// Gets the total count of published blogs for a specific language.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <returns>The total count of published blogs.</returns>
        Task<int> GetPublishedBlogsCountAsync(string languageCode);

        /// <summary>
        /// Searches for published blogs based on a search term.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <param name="take">The number of items to take.</param>
        /// <returns>A list of blog content matching the search term.</returns>
        Task<List<BlogContent>> SearchBlogsAsync(string languageCode, string searchTerm, int skip = 0, int take = 10);

        /// <summary>
        /// Gets the count of blogs matching a search term.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>The count of matching blogs.</returns>
        Task<int> SearchBlogsCountAsync(string languageCode, string searchTerm);

        /// <summary>
        /// Retrieves blogs associated with a specific tag.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="tag">The tag to filter by.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <param name="take">The number of items to take.</param>
        /// <returns>A list of blog content with the specified tag.</returns>
        Task<List<BlogContent>> GetBlogsByTagAsync(string languageCode, string tag, int skip = 0, int take = 10);

        /// <summary>
        /// Gets the count of blogs associated with a specific tag.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="tag">The tag to filter by.</param>
        /// <returns>The count of matching blogs.</returns>
        Task<int> GetBlogsByTagCountAsync(string languageCode, string tag);

        /// <summary>
        /// Retrieves blogs published in a specific month and year.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <param name="take">The number of items to take.</param>
        /// <returns>A list of blog content from the specified month.</returns>
        Task<List<BlogContent>> GetBlogsByMonthAsync(string languageCode, int year, int month, int skip = 0, int take = 10);

        /// <summary>
        /// Gets the count of blogs published in a specific month and year.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <returns>The count of matching blogs.</returns>
        Task<int> GetBlogsByMonthCountAsync(string languageCode, int year, int month);

        /// <summary>
        /// Retrieves the most popular tags for a specific language.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="count">The number of tags to return.</param>
        /// <returns>A list of the most popular tags.</returns>
        Task<List<string>> GetPopularTagsAsync(string languageCode, int count = 10);

        /// <summary>
        /// Gets a list of months that have blog posts, for archive purposes.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <returns>A list of view models representing the archive months.</returns>
        Task<List<ArchiveMonthViewModel>> GetArchiveMonthsAsync(string languageCode);

        /// <summary>
        /// Retrieves blog content by its URL slug.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="slug">The URL slug.</param>
        /// <returns>The blog content, or null if not found.</returns>
        Task<BlogContent?> GetBlogContentBySlugAsync(string languageCode, string slug);
    }
} 