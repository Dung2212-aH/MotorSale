using BaseCore.LogService.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCore.LogService
{
    public interface ILogErrorService
    {
        Task<ICollection<LogError>> GetAllListAsync();
        Task CreateAsync(LogError logError);
        Task CreateLog(HttpContext httpContext, string message);
    }

    public class LogErrorService : ILogErrorService
    {
        private readonly LogDbContext _context;

        public LogErrorService(LogDbContext context)
        {
            _context = context;
        }

        public async Task CreateLog(HttpContext httpContext, string message)
        {
            var requestBody = string.Empty;
            httpContext.Request.EnableBuffering();
            using (var reader = new StreamReader(httpContext.Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            var pathUrl = string.Format("{0}://{1}{2}", httpContext.Request.Scheme, httpContext.Request.Host, httpContext.Request.Path);
            var logError = new LogError
            {
                Header = $"REQUEST HttpMethod: {httpContext.Request.Method}, Path: {pathUrl}, Content-Type: {httpContext.Request.ContentType}",
                Body = requestBody,
                CreatedUser = httpContext.User.Identity?.Name,
                CreatedDateTime = DateTime.UtcNow,
                Message = message
            };

            await CreateAsync(logError);
        }

        public async Task<ICollection<LogError>> GetAllListAsync()
        {
            return await _context.LogErrors
                .OrderByDescending(x => x.CreatedDateTime)
                .ToListAsync();
        }

        public async Task CreateAsync(LogError logError)
        {
            logError.CreatedDateTime = logError.CreatedDateTime == default ? DateTime.UtcNow : logError.CreatedDateTime;
            _context.LogErrors.Add(logError);
            await _context.SaveChangesAsync();
        }
    }
}
