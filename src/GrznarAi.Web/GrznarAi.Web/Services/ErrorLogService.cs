using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Batch/timed error and warning logging service
    /// </summary>
    public class ErrorLogService : IErrorLogService, IDisposable
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ConcurrentQueue<ErrorLog> _logQueue = new();
        private readonly Timer _timer;
        private readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(30); // lze upravit
        private readonly int _batchSize = 20; // lze upravit
        private bool _isFlushing = false;
        private bool _disposed = false;

        public ErrorLogService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            _timer = new Timer(async _ => await FlushAsync(), null, _flushInterval, _flushInterval);
        }

        public async Task LogAsync(string message, string? stackTrace = null, string? innerException = null, string? level = null, string? source = null)
        {
            var log = new ErrorLog
            {
                Message = message,
                StackTrace = stackTrace,
                InnerException = innerException,
                Level = level,
                Source = source,
                CreatedAt = DateTime.UtcNow
            };
            _logQueue.Enqueue(log);
            if (_logQueue.Count >= _batchSize)
            {
                await FlushAsync();
            }
        }

        public async Task FlushAsync()
        {
            if (_isFlushing) return;
            _isFlushing = true;
            try
            {
                var logsToSave = new List<ErrorLog>();
                while (_logQueue.TryDequeue(out var log))
                {
                    logsToSave.Add(log);
                    if (logsToSave.Count >= _batchSize)
                        break;
                }
                if (logsToSave.Count > 0)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    context.ErrorLogs.AddRange(logsToSave);
                    await context.SaveChangesAsync();
                }
            }
            finally
            {
                _isFlushing = false;
            }
        }

        public async Task<List<ErrorLog>> GetLogsAsync(int page, int pageSize, string? search = null, string? level = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.ErrorLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Message.Contains(search) || (e.StackTrace != null && e.StackTrace.Contains(search)));
            }
            if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(e => e.Level == level);
            }
            return await query.OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetLogsCountAsync(string? search = null, string? level = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.ErrorLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Message.Contains(search) || (e.StackTrace != null && e.StackTrace.Contains(search)));
            }
            if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(e => e.Level == level);
            }
            return await query.CountAsync();
        }

        public async Task<bool> DeleteLogAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var log = await context.ErrorLogs.FindAsync(id);
            if (log == null) return false;
            context.ErrorLogs.Remove(log);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAllLogsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var all = await context.ErrorLogs.ToListAsync();
            context.ErrorLogs.RemoveRange(all);
            await context.SaveChangesAsync();
            return all.Count;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer.Dispose();
                _disposed = true;
            }
        }
    }
} 