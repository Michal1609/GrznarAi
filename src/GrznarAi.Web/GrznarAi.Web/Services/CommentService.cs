using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface ICommentService
    {
        Task<List<Comment>> GetCommentsForBlogAsync(int blogId, bool includeReplies = true);
        Task<Comment> AddCommentAsync(Comment comment);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<bool> ApproveCommentAsync(int commentId);
        Task<bool> AddOrUpdateVoteAsync(int commentId, bool isLike, string? userId, string? cookieId);
        Task<bool> RemoveVoteAsync(int commentId, string? userId, string? cookieId);
        Task<bool> AddOrUpdateBlogVoteAsync(int blogId, bool isLike, string? userId, string? cookieId);
        Task<bool> RemoveBlogVoteAsync(int blogId, string? userId, string? cookieId);
        Task<(bool HasVoted, bool? IsLike)> GetUserVoteForCommentAsync(int commentId, string? userId, string? cookieId);
        Task<(bool HasVoted, bool? IsLike)> GetUserVoteForBlogAsync(int blogId, string? userId, string? cookieId);
    }

    public class CommentService : ICommentService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CommentService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Model pro návratovou hodnotu filtrovaných komentářů
        public class FilteredCommentsResult
        {
            public List<Comment> Comments { get; set; } = new List<Comment>();
            public int TotalCount { get; set; }
        }

        // Metoda pro administrační panel komentářů
        public async Task<FilteredCommentsResult> GetFilteredCommentsAsync(
            string searchText, 
            string statusFilter, 
            int blogId, 
            int skip, 
            int take)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Vytvoříme základní dotaz
            var query = context.Comments.AsQueryable();
            
            // Filtr podle blogu
            if (blogId > 0)
            {
                query = query.Where(c => c.BlogId == blogId);
            }
            
            // Filtr podle statusu
            switch (statusFilter)
            {
                case "approved":
                    query = query.Where(c => c.IsApproved && !c.IsDeleted);
                    break;
                case "unapproved":
                    query = query.Where(c => !c.IsApproved && !c.IsDeleted);
                    break;
                case "deleted":
                    query = query.Where(c => c.IsDeleted);
                    break;
                default: // all
                    break;
            }
            
            // Filtr podle vyhledávacího textu
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string search = searchText.ToLower();
                query = query.Where(c => 
                    c.Content.ToLower().Contains(search) || 
                    c.AuthorName.ToLower().Contains(search) ||
                    (c.AuthorEmail != null && c.AuthorEmail.ToLower().Contains(search)));
            }
            
            // Získáme celkový počet komentářů splňujících filtr
            int totalCount = await query.CountAsync();
            
            // Získáme stránkovaný výsledek
            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(c => c.Blog)
                .Include(c => c.User)
                .ToListAsync();
            
            return new FilteredCommentsResult
            {
                Comments = comments,
                TotalCount = totalCount
            };
        }

        public async Task<List<Comment>> GetCommentsForBlogAsync(int blogId, bool includeReplies = true)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Získej komentáře prvního úrovně (bez nadřazeného komentáře)
            var query = context.Comments
                .Where(c => c.BlogId == blogId && !c.IsDeleted && c.ParentCommentId == null);

            if (includeReplies)
            {
                // Včetně podkomentářů
                query = query.Include(c => c.Replies.Where(r => !r.IsDeleted))
                             .ThenInclude(r => r.Replies.Where(r2 => !r2.IsDeleted));
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            context.Comments.Add(comment);
            
            // Aktualizuj počet komentářů u blogu
            var blog = await context.Blogs.FindAsync(comment.BlogId);
            if (blog != null)
            {
                blog.CommentsCount++;
            }
            
            await context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Comments
                .Include(c => c.Replies.Where(r => !r.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var existingComment = await context.Comments.FindAsync(comment.Id);
            if (existingComment == null || existingComment.IsDeleted)
            {
                return false;
            }

            existingComment.Content = comment.Content;
            existingComment.UpdatedAt = DateTime.UtcNow;
            
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var comment = await context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return false;
            }

            // Místo fyzického smazání nastavíme příznak IsDeleted
            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;
            
            // Aktualizuj počet komentářů u blogu
            var blog = await context.Blogs.FindAsync(comment.BlogId);
            if (blog != null)
            {
                blog.CommentsCount--;
            }
            
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveCommentAsync(int commentId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var comment = await context.Comments.FindAsync(commentId);
            if (comment == null || comment.IsDeleted)
            {
                return false;
            }

            comment.IsApproved = true;
            comment.UpdatedAt = DateTime.UtcNow;
            
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddOrUpdateVoteAsync(int commentId, bool isLike, string? userId, string? cookieId)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(cookieId))
            {
                return false; // Musí být zadán buď userId nebo cookieId
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            
            var comment = await context.Comments.FindAsync(commentId);
            if (comment == null || comment.IsDeleted)
            {
                return false;
            }

            // Najdi existující hlasování
            var existingVote = await context.CommentVotes
                .FirstOrDefaultAsync(v => v.CommentId == commentId && 
                    (v.ApplicationUserId == userId || v.UserCookieId == cookieId));

            if (existingVote != null)
            {
                // Pokud uživatel mění svůj hlas
                if (existingVote.IsLike != isLike)
                {
                    // Odeber starý hlas z počítadla
                    if (existingVote.IsLike)
                    {
                        comment.LikesCount--;
                    }
                    else
                    {
                        comment.DislikesCount--;
                    }

                    // Přidej nový hlas do počítadla
                    if (isLike)
                    {
                        comment.LikesCount++;
                    }
                    else
                    {
                        comment.DislikesCount++;
                    }

                    // Aktualizuj hlas
                    existingVote.IsLike = isLike;
                    existingVote.UpdatedAt = DateTime.UtcNow;
                }
                // Pokud uživatel hlasuje stejně, nic se nemění
            }
            else
            {
                // Vytvoř nový hlas
                var newVote = new CommentVote
                {
                    CommentId = commentId,
                    ApplicationUserId = userId,
                    UserCookieId = cookieId,
                    IsLike = isLike,
                    CreatedAt = DateTime.UtcNow
                };

                context.CommentVotes.Add(newVote);

                // Aktualizuj počítadlo
                if (isLike)
                {
                    comment.LikesCount++;
                }
                else
                {
                    comment.DislikesCount++;
                }
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveVoteAsync(int commentId, string? userId, string? cookieId)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(cookieId))
            {
                return false; // Musí být zadán buď userId nebo cookieId
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Najdi existující hlasování
            var existingVote = await context.CommentVotes
                .FirstOrDefaultAsync(v => v.CommentId == commentId && 
                    (v.ApplicationUserId == userId || v.UserCookieId == cookieId));

            if (existingVote == null)
            {
                return false; // Nemůžeme odstranit neexistující hlas
            }

            // Najdi komentář
            var comment = await context.Comments.FindAsync(commentId);
            if (comment == null || comment.IsDeleted)
            {
                return false;
            }

            // Aktualizuj počítadlo
            if (existingVote.IsLike)
            {
                comment.LikesCount = Math.Max(0, comment.LikesCount - 1);
            }
            else
            {
                comment.DislikesCount = Math.Max(0, comment.DislikesCount - 1);
            }

            // Odstraň hlasování
            context.CommentVotes.Remove(existingVote);
            
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddOrUpdateBlogVoteAsync(int blogId, bool isLike, string? userId, string? cookieId)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(cookieId))
            {
                return false; // Musí být zadán buď userId nebo cookieId
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            
            var blog = await context.Blogs.FindAsync(blogId);
            if (blog == null)
            {
                return false;
            }

            // Najdi existující hlasování
            var existingVote = await context.BlogVotes
                .FirstOrDefaultAsync(v => v.BlogId == blogId && 
                    (v.ApplicationUserId == userId || v.UserCookieId == cookieId));

            if (existingVote != null)
            {
                // Pokud uživatel mění svůj hlas
                if (existingVote.IsLike != isLike)
                {
                    // Odeber starý hlas z počítadla
                    if (existingVote.IsLike)
                    {
                        blog.LikesCount--;
                    }
                    else
                    {
                        blog.DislikesCount--;
                    }

                    // Přidej nový hlas do počítadla
                    if (isLike)
                    {
                        blog.LikesCount++;
                    }
                    else
                    {
                        blog.DislikesCount++;
                    }

                    // Aktualizuj hlas
                    existingVote.IsLike = isLike;
                    existingVote.UpdatedAt = DateTime.UtcNow;
                }
                // Pokud uživatel hlasuje stejně, nic se nemění
            }
            else
            {
                // Vytvoř nový hlas
                var newVote = new BlogVote
                {
                    BlogId = blogId,
                    ApplicationUserId = userId,
                    UserCookieId = cookieId,
                    IsLike = isLike,
                    CreatedAt = DateTime.UtcNow
                };

                context.BlogVotes.Add(newVote);

                // Aktualizuj počítadlo
                if (isLike)
                {
                    blog.LikesCount++;
                }
                else
                {
                    blog.DislikesCount++;
                }
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveBlogVoteAsync(int blogId, string? userId, string? cookieId)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(cookieId))
            {
                return false; // Musí být zadán buď userId nebo cookieId
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Najdi existující hlasování
            var existingVote = await context.BlogVotes
                .FirstOrDefaultAsync(v => v.BlogId == blogId && 
                    (v.ApplicationUserId == userId || v.UserCookieId == cookieId));

            if (existingVote == null)
            {
                return false; // Nemůžeme odstranit neexistující hlas
            }

            // Najdi blog
            var blog = await context.Blogs.FindAsync(blogId);
            if (blog == null)
            {
                return false;
            }

            // Aktualizuj počítadlo
            if (existingVote.IsLike)
            {
                blog.LikesCount = Math.Max(0, blog.LikesCount - 1);
            }
            else
            {
                blog.DislikesCount = Math.Max(0, blog.DislikesCount - 1);
            }

            // Odstraň hlasování
            context.BlogVotes.Remove(existingVote);
            
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool HasVoted, bool? IsLike)> GetUserVoteForCommentAsync(int commentId, string? userId, string? cookieId)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(cookieId))
            {
                return (false, null);
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            
            var vote = await context.CommentVotes
                .FirstOrDefaultAsync(v => v.CommentId == commentId && 
                    (v.ApplicationUserId == userId || v.UserCookieId == cookieId));

            return vote != null ? (true, vote.IsLike) : (false, null);
        }

        public async Task<(bool HasVoted, bool? IsLike)> GetUserVoteForBlogAsync(int blogId, string? userId, string? cookieId)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(cookieId))
            {
                return (false, null);
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            
            var vote = await context.BlogVotes
                .FirstOrDefaultAsync(v => v.BlogId == blogId && 
                    (v.ApplicationUserId == userId || v.UserCookieId == cookieId));

            return vote != null ? (true, vote.IsLike) : (false, null);
        }
    }
} 