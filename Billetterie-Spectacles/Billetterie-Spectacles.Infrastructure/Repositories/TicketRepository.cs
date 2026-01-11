using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Billetterie_Spectacles.Infrastructure.Repositories
{
    /// <summary>
    /// Implémentation du repository Ticket avec Entity Framework Core
    /// </summary>
    public class TicketRepository(BilletterieDbContext _context) : ITicketRepository
    {
        #region CRUD Operations

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == id);
        }

        public async Task<IEnumerable<Ticket?>> GetAllAsync()
        {
            return await _context.Tickets
                .OrderByDescending(t => t.TicketId)
                .ToListAsync();
        }

        public async Task<Ticket?> AddAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket> UpdateAsync(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await GetByIdAsync(id);
            if (ticket == null)
                return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Business Methods

        public async Task<IEnumerable<Ticket>> GetByOrderIdAsync(int orderId)
        {
            return await _context.Tickets
                .Where(t => t.OrderId == orderId)
                .OrderBy(t => t.TicketId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByOrderWithPerformanceAsync(int orderId)
        {
            return await _context.Tickets
                .Include(t => t.Performance)
                    .ThenInclude(p => p.Spectacle)
                .Where(t => t.OrderId == orderId)
                .OrderBy(t => t.TicketId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByPerformanceIdAsync(int performanceId)
        {
            return await _context.Tickets
                .Where(t => t.PerformanceId == performanceId)
                .OrderBy(t => t.TicketId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status)
        {
            return await _context.Tickets
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.TicketId)
                .ToListAsync();
        }

        public async Task<int> CountByPerformanceIdAsync(int performanceId)
        {
            return await _context.Tickets
                .CountAsync(t => t.PerformanceId == performanceId);
        }

        #endregion
    }
}
