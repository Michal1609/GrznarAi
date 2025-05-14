using GrznarAi.Web.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Interface for batch/timed error and warning logging service
    /// </summary>
    public interface IErrorLogService
    {
        /// <summary>
        /// Log error or warning (adds to batch, not saved immediately)
        /// </summary>
        Task LogAsync(string message, string? stackTrace = null, string? innerException = null, string? level = null, string? source = null);

        /// <summary>
        /// Immediately flushes all batched logs to the database
        /// </summary>
        Task FlushAsync();

        /// <summary>
        /// Get paged error logs
        /// </summary>
        Task<List<ErrorLog>> GetLogsAsync(int page, int pageSize, string? search = null, string? level = null);

        /// <summary>
        /// Get total count of error logs
        /// </summary>
        Task<int> GetLogsCountAsync(string? search = null, string? level = null);

        /// <summary>
        /// Delete a log by id
        /// </summary>
        Task<bool> DeleteLogAsync(int id);

        /// <summary>
        /// Delete all logs
        /// </summary>
        Task<int> DeleteAllLogsAsync();
    }
} 