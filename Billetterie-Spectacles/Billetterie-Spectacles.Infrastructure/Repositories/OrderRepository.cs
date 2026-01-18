using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Billetterie_Spectacles.Infrastructure.Repositories
{
    // <summary>
    /// Implémentation du repository Order avec Entity Framework Core
    /// </summary>
    public class OrderRepository(BilletterieDbContext _context) : IOrderRepository
    {

        #region CRUD Operations

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<IEnumerable<Order?>> GetAllAsync()
        {
            return await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Order? order = await GetByIdAsync(id);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Business Methods

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // <summary>
        /// Récupère toutes les commandes avec filtres optionnels (pour Admin et organisateur)
        /// </summary>
        public async Task<IEnumerable<Order>> GetAllWithFiltersAsync(
            int? userId = null,
            int? performanceId = null,
            OrderStatus? orderStatus = null)
        {
            var query = _context.Orders
                .Include(o => o.Tickets)                // on charge les tickets
                    .ThenInclude(p => p.Performance)    // on charge les représentations correspondantes
                    .ThenInclude(p => p.Spectacle)      // on charge les spectacles correspondants
                .Include(o => o.User)
                .Include(o => o.Tickets)
                .AsQueryable();

            // Appliquer les filtres optionnels
            if (userId.HasValue)
                query = query.Where(o => o.UserId == userId.Value);

            if(performanceId.HasValue)
        query = query.Where(o => o.Tickets.Any(t => t.PerformanceId == performanceId.Value));

            if (orderStatus.HasValue)
                query = query.Where(o => o.Status == orderStatus.Value);

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetWithTicketsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.Performance)
                        .ThenInclude(p => p.Spectacle)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.PaymentConfirmed)                      // Les recettes ne se font que sur les commandes payées
                .SumAsync(o => o.TotalPrice);                                       // methode LINQ pour l'aggregation des valeurs
        }

        public async Task<decimal> GetTotalRevenueByUserAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.PaymentConfirmed) // Les recettes ne se font que sur les commandes payées
                .SumAsync(o => o.TotalPrice);
        }

        public async Task<int> CountByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .CountAsync(o => o.Status == status);
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)                        // selectionne les 10 dernières commandes
                .ToListAsync();
        }

        #endregion
    }
}
