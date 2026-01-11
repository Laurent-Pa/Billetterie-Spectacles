using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Billetterie_Spectacles.Infrastructure.Repositories
{
    /// <summary>
    /// Implémentation du repository Performance avec Entity Framework Core
    /// Injection de dépendance avec variable privée _context
    /// </summary>
    public class PerformanceRepository(BilletterieDbContext _context) : IPerformanceRepository
    {

        #region CRUD Operations

        public async Task<Performance?> GetByIdAsync(int id)
        {
            return await _context.Performances
                .FirstOrDefaultAsync(p => p.PerformanceId == id);
        }

        public async Task<IEnumerable<Performance?>> GetAllAsync()
        {
            return await _context.Performances
                .OrderBy(p => p.Date)
                .ToListAsync();
        }

        public async Task<Performance?> AddAsync(Performance performance)
        {
            _context.Performances.Add(performance);
            await _context.SaveChangesAsync();
            return performance;
        }

        public async Task<Performance> UpdateAsync(Performance performance)
        {
            _context.Performances.Update(performance);
            await _context.SaveChangesAsync();
            return performance;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Performance? performance = await GetByIdAsync(id);
            if (performance == null)
                return false;

            _context.Performances.Remove(performance);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Business Methods

        public async Task<IEnumerable<Performance>> GetBySpectacleIdAsync(int spectacleId)
        {
            return await _context.Performances
                .Where(p => p.SpectacleId == spectacleId)
                .OrderBy(p => p.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Performance>> GetByStatusAsync(PerformanceStatus status)
        {
            return await _context.Performances
                .Where(p => p.Status == status)
                .OrderBy(p => p.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Performance>> GetUpcomingAsync()
        {
            return await _context.Performances
                .Where(p => p.Date > DateTime.UtcNow)
                .OrderBy(p => p.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Performance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Performances
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .OrderBy(p => p.Date)
                .ToListAsync();
        }

        public async Task<Performance?> GetWithTicketsAsync(int id)
        {
            return await _context.Performances
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(p => p.PerformanceId == id);
        }

        public async Task<bool> HasAvailableTicketsAsync(int performanceId)
        {
            Performance? performance = await GetByIdAsync(performanceId);
            return performance != null && performance.AvailableTickets > 0;
        }

        public async Task<int> GetAvailableTicketsCountAsync(int performanceId)
        {
            int countPerformance = await _context.Performances
                .Where(p => p.PerformanceId == performanceId)
                .Select(p => p.AvailableTickets)
                .FirstOrDefaultAsync();

            return countPerformance;
        }

        #endregion
    }
}
