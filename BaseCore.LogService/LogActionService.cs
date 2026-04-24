using BaseCore.LogService.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCore.LogService
{
    public interface ILogActionService
    {
        Task<ICollection<LogAction>> GetAllListAsync();
        Task CreateAsync(LogAction logAction);
        Task CreateLog(LogAction logAction);
    }

    public class LogActionService : ILogActionService
    {
        private readonly LogDbContext _context;

        public LogActionService(LogDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<LogAction>> GetAllListAsync()
        {
            return await _context.LogActions
                .OrderByDescending(x => x.CreatedDateTime)
                .ToListAsync();
        }

        public async Task CreateAsync(LogAction logAction)
        {
            logAction.CreatedDateTime = logAction.CreatedDateTime == default ? DateTime.UtcNow : logAction.CreatedDateTime;
            _context.LogActions.Add(logAction);
            await _context.SaveChangesAsync();
        }

        public async Task CreateLog(LogAction logAction)
        {
            await CreateAsync(logAction);
        }
    }
}
