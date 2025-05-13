using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public class AiNewsErrorService : IAiNewsErrorService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AiNewsErrorService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<AiNewsError>> GetErrorsAsync(int page, int pageSize, string searchTerm = null, int? sourceId = null, string category = null, bool? isResolved = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.AiNewsErrors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(e => 
                    e.Message.ToLower().Contains(searchTerm) || 
                    (e.Details != null && e.Details.ToLower().Contains(searchTerm)) ||
                    (e.Resolution != null && e.Resolution.ToLower().Contains(searchTerm)));
            }

            if (sourceId.HasValue)
            {
                query = query.Where(e => e.SourceId == sourceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(e => e.Category == category);
            }

            if (isResolved.HasValue)
            {
                query = query.Where(e => e.IsResolved == isResolved.Value);
            }

            return await query
                .OrderByDescending(e => e.OccurredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetErrorsCountAsync(string searchTerm = null, int? sourceId = null, string category = null, bool? isResolved = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.AiNewsErrors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(e => 
                    e.Message.ToLower().Contains(searchTerm) || 
                    (e.Details != null && e.Details.ToLower().Contains(searchTerm)) ||
                    (e.Resolution != null && e.Resolution.ToLower().Contains(searchTerm)));
            }

            if (sourceId.HasValue)
            {
                query = query.Where(e => e.SourceId == sourceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(e => e.Category == category);
            }

            if (isResolved.HasValue)
            {
                query = query.Where(e => e.IsResolved == isResolved.Value);
            }

            return await query.CountAsync();
        }

        public async Task<AiNewsError> GetErrorByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AiNewsErrors.FindAsync(id);
        }

        public async Task<int> AddErrorAsync(AiNewsError error)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.AiNewsErrors.Add(error);
            await context.SaveChangesAsync();
            return error.Id;
        }

        public async Task AddErrorsAsync(IEnumerable<AiNewsError> errors)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.AiNewsErrors.AddRange(errors);
            await context.SaveChangesAsync();
        }

        public async Task<bool> MarkAsResolvedAsync(int id, string resolution)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var error = await context.AiNewsErrors.FindAsync(id);
            if (error == null)
                return false;

            error.IsResolved = true;
            error.ResolvedAt = DateTime.UtcNow;
            error.Resolution = resolution;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteErrorAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var error = await context.AiNewsErrors.FindAsync(id);
            if (error == null)
                return false;

            context.AiNewsErrors.Remove(error);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteResolvedErrorsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var resolvedErrors = await context.AiNewsErrors
                .Where(e => e.IsResolved)
                .ToListAsync();

            context.AiNewsErrors.RemoveRange(resolvedErrors);
            await context.SaveChangesAsync();
            return resolvedErrors.Count;
        }

        public async Task<int> DeleteAllErrorsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var errors = await context.AiNewsErrors.ToListAsync();
            
            context.AiNewsErrors.RemoveRange(errors);
            await context.SaveChangesAsync();
            return errors.Count;
        }

        public async Task<int> GetUnresolvedErrorsCountAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AiNewsErrors
                .CountAsync(e => !e.IsResolved);
        }
    }
} 