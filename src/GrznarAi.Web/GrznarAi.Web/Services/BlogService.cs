using Microsoft.EntityFrameworkCore;
using GrznarAi.Web.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GrznarAi.Web.Services
{
    public class BlogService : IBlogService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BlogService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Blog>> GetBlogsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Blogs
                .Include(b => b.Contents)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Blog?> GetBlogByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Blogs
                .Include(b => b.Contents)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Blog> CreateBlogAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var blog = new Blog();
            context.Blogs.Add(blog);
            await context.SaveChangesAsync();
            return blog;
        }

        public async Task DeleteBlogAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var blog = await context.Blogs
                .Include(b => b.Contents)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog != null)
            {
                // Nejprve odstraníme všechny obsahy
                context.BlogContents.RemoveRange(blog.Contents);
                // Poté odstraníme blog
                context.Blogs.Remove(blog);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Blog with ID {id} not found.");
            }
        }

        public async Task<BlogContent?> GetBlogContentAsync(int blogId, string languageCode)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogContents
                .FirstOrDefaultAsync(bc => bc.BlogId == blogId && bc.LanguageCode == languageCode);
        }

        public async Task<List<BlogContent>> GetBlogContentsByLanguageAsync(string languageCode)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogContents
                .Where(bc => bc.LanguageCode == languageCode)
                .OrderByDescending(bc => bc.CreatedAt)
                .ToListAsync();
        }

        public async Task<BlogContent> CreateOrUpdateBlogContentAsync(BlogContent blogContent)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var existingContent = await context.BlogContents
                .FirstOrDefaultAsync(bc => bc.BlogId == blogContent.BlogId && bc.LanguageCode == blogContent.LanguageCode);

            if (existingContent == null)
            {
                // Vytvoření nového obsahu
                blogContent.CreatedAt = DateTime.UtcNow;
                context.BlogContents.Add(blogContent);
            }
            else
            {
                // Aktualizace existujícího obsahu
                existingContent.Title = blogContent.Title;
                existingContent.Description = blogContent.Description;
                existingContent.Content = blogContent.Content;
                existingContent.Tags = blogContent.Tags;
                existingContent.IsPublished = blogContent.IsPublished;
                existingContent.UpdatedAt = DateTime.UtcNow;
                
                blogContent = existingContent; // Pro návratový účel
            }

            // Aktualizace časové značky u blog entity
            var blog = await context.Blogs.FindAsync(blogContent.BlogId);
            if (blog != null)
            {
                blog.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return blogContent;
        }

        public async Task DeleteBlogContentAsync(int blogContentId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var blogContent = await context.BlogContents.FindAsync(blogContentId);

            if (blogContent != null)
            {
                context.BlogContents.Remove(blogContent);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"BlogContent with ID {blogContentId} not found.");
            }
        }
        
        // Implementace metod pro veřejnou část blogu
        
        public async Task<List<BlogContent>> GetPublishedBlogsAsync(string languageCode, int skip = 0, int take = 10)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogContents
                .Include(bc => bc.Blog)
                .Where(bc => bc.LanguageCode == languageCode && bc.IsPublished)
                .OrderByDescending(bc => bc.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        
        public async Task<int> GetPublishedBlogsCountAsync(string languageCode)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogContents
                .Where(bc => bc.LanguageCode == languageCode && bc.IsPublished)
                .CountAsync();
        }
        
        public async Task<List<BlogContent>> SearchBlogsAsync(string languageCode, string searchTerm, int skip = 0, int take = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetPublishedBlogsAsync(languageCode, skip, take);
                
            searchTerm = searchTerm.ToLower();
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // Získáme všechny publikované blogy v daném jazyce
            var blogsQuery = context.BlogContents
                .Include(bc => bc.Blog)
                .Where(bc => bc.LanguageCode == languageCode && bc.IsPublished);
            
            // Vyhledáme ty, které obsahují hledaný text v některém z polí
            var filteredBlogs = await blogsQuery.ToListAsync();
            
            var results = filteredBlogs
                .Where(bc => 
                    bc.Title.ToLower().Contains(searchTerm) || 
                    (bc.Description != null && bc.Description.ToLower().Contains(searchTerm)) || 
                    bc.Content.ToLower().Contains(searchTerm) || 
                    (bc.Tags != null && bc.Tags.ToLower().Contains(searchTerm)))
                .OrderByDescending(bc => bc.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToList();
                
            return results;
        }
        
        public async Task<int> SearchBlogsCountAsync(string languageCode, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetPublishedBlogsCountAsync(languageCode);
                
            searchTerm = searchTerm.ToLower();
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // Získáme všechny publikované blogy v daném jazyce
            var blogsQuery = context.BlogContents
                .Where(bc => bc.LanguageCode == languageCode && bc.IsPublished);
            
            // Vyhledáme ty, které obsahují hledaný text v některém z polí
            var filteredBlogs = await blogsQuery.ToListAsync();
            
            return filteredBlogs
                .Count(bc => 
                    bc.Title.ToLower().Contains(searchTerm) || 
                    (bc.Description != null && bc.Description.ToLower().Contains(searchTerm)) || 
                    bc.Content.ToLower().Contains(searchTerm) || 
                    (bc.Tags != null && bc.Tags.ToLower().Contains(searchTerm)));
        }
        
        public async Task<List<BlogContent>> GetBlogsByTagAsync(string languageCode, string tag, int skip = 0, int take = 10)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogContents
                .Include(bc => bc.Blog)
                .Where(bc => 
                    bc.LanguageCode == languageCode && 
                    bc.IsPublished && 
                    bc.Tags.Contains(tag))
                .OrderByDescending(bc => bc.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        
        public async Task<int> GetBlogsByTagCountAsync(string languageCode, string tag)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogContents
                .Where(bc => 
                    bc.LanguageCode == languageCode && 
                    bc.IsPublished && 
                    bc.Tags.Contains(tag))
                .CountAsync();
        }
        
        public async Task<List<BlogContent>> GetBlogsByMonthAsync(string languageCode, int year, int month, int skip = 0, int take = 10)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogContents
                .Include(bc => bc.Blog)
                .Where(bc => 
                    bc.LanguageCode == languageCode && 
                    bc.IsPublished && 
                    bc.CreatedAt >= startDate &&
                    bc.CreatedAt < endDate)
                .OrderByDescending(bc => bc.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        
        public async Task<int> GetBlogsByMonthCountAsync(string languageCode, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogContents
                .Where(bc => 
                    bc.LanguageCode == languageCode && 
                    bc.IsPublished && 
                    bc.CreatedAt >= startDate &&
                    bc.CreatedAt < endDate)
                .CountAsync();
        }
        
        public async Task<List<string>> GetPopularTagsAsync(string languageCode, int count = 10)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var blogContents = await context.BlogContents
                .Where(bc => bc.LanguageCode == languageCode && bc.IsPublished && !string.IsNullOrEmpty(bc.Tags))
                .ToListAsync();
                
            // Extrakce a počítání tagů
            var tagDictionary = new Dictionary<string, int>();
            
            foreach (var content in blogContents)
            {
                if (string.IsNullOrEmpty(content.Tags)) continue;
                
                var tags = content.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t));
                    
                foreach (var tag in tags)
                {
                    if (tagDictionary.ContainsKey(tag))
                        tagDictionary[tag]++;
                    else
                        tagDictionary[tag] = 1;
                }
            }
            
            // Vrácení nejpopulárnějších tagů
            return tagDictionary
                .OrderByDescending(kv => kv.Value)
                .Take(count)
                .Select(kv => kv.Key)
                .ToList();
        }
        
        public async Task<List<DateTime>> GetArchiveMonthsAsync(string languageCode)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var dates = await context.BlogContents
                .Where(bc => bc.LanguageCode == languageCode && bc.IsPublished)
                .Select(bc => bc.CreatedAt)
                .Distinct()
                .ToListAsync();
                
            // Seskupení podle roku a měsíce
            return dates
                .GroupBy(d => new { d.Year, d.Month })
                .Select(g => new DateTime(g.Key.Year, g.Key.Month, 1))
                .OrderByDescending(d => d)
                .ToList();
        }
        
        public async Task<BlogContent?> GetBlogContentBySlugAsync(string languageCode, string slug)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            // Slug není v modelu, takže musíme použít alternativní strategii, např. slug = normalizovaný Title
            var blogContents = await context.BlogContents
                .Include(bc => bc.Blog)
                .Where(bc => bc.LanguageCode == languageCode && bc.IsPublished)
                .ToListAsync();
                
            // Vytvoření slugu z titulku a porovnání
            return blogContents.FirstOrDefault(bc => 
                GenerateSlug(bc.Title) == slug);
        }
        
        // Pomocná metoda pro generování slugu z titulku
        private string GenerateSlug(string title)
        {
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