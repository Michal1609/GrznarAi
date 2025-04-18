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
    }
} 