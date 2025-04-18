using GrznarAi.Web.Data;
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
    }
} 