using GrznarAi.Web.Data;
using GrznarAi.Web.Components.Pages.Blog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface IBlogService
    {
        Task<List<Blog>> GetBlogsAsync();
        Task<Blog?> GetBlogByIdAsync(int id);
        Task<Blog> CreateBlogAsync();
        Task DeleteBlogAsync(int id);

        // Metody pro práci s vícejazyčným obsahem blogu
        Task<BlogContent?> GetBlogContentAsync(int blogId, string languageCode);
        Task<List<BlogContent>> GetBlogContentsByLanguageAsync(string languageCode);
        Task<BlogContent> CreateOrUpdateBlogContentAsync(BlogContent blogContent);
        Task DeleteBlogContentAsync(int blogContentId);
        
        // Metody pro veřejné zobrazení blogu
        Task<List<BlogContent>> GetPublishedBlogsAsync(string languageCode, int skip = 0, int take = 10);
        Task<int> GetPublishedBlogsCountAsync(string languageCode);
        Task<List<BlogContent>> SearchBlogsAsync(string languageCode, string searchTerm, int skip = 0, int take = 10);
        Task<int> SearchBlogsCountAsync(string languageCode, string searchTerm);
        Task<List<BlogContent>> GetBlogsByTagAsync(string languageCode, string tag, int skip = 0, int take = 10);
        Task<int> GetBlogsByTagCountAsync(string languageCode, string tag);
        Task<List<BlogContent>> GetBlogsByMonthAsync(string languageCode, int year, int month, int skip = 0, int take = 10);
        Task<int> GetBlogsByMonthCountAsync(string languageCode, int year, int month);
        Task<List<string>> GetPopularTagsAsync(string languageCode, int count = 10);
        Task<List<ArchiveMonthViewModel>> GetArchiveMonthsAsync(string languageCode);
        Task<BlogContent?> GetBlogContentBySlugAsync(string languageCode, string slug);
    }
} 