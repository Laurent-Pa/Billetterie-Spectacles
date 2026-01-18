using Billeterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Billetterie_Spectacles.Infrastructure.Repositories
{
    /// <summary>
    /// Implémentation du repository Spectacle avec Entity Framework Core
    /// Injection de dépendance avec variable privée _context
    /// </summary>
    public class SpectacleRepository(BilletterieDbContext _context) : ISpectacleRepository
    {

        #region CRUD Operations

        public async Task<Spectacle?> GetByIdAsync(int id)
        {
            return await _context.Spectacles
                .FirstOrDefaultAsync(s => s.SpectacleId == id);
        }

        public async Task<IEnumerable<Spectacle>> GetAllAsync()
        {
            return await _context.Spectacles
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Spectacle?> AddAsync(Spectacle spectacle)
        {
            _context.Spectacles.Add(spectacle);
            await _context.SaveChangesAsync();
            return spectacle;
        }

        public async Task<Spectacle> UpdateAsync(Spectacle spectacle)
        {
            _context.Spectacles.Update(spectacle);
            await _context.SaveChangesAsync();
            return spectacle;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Spectacle? spectacle = await GetByIdAsync(id);
            if (spectacle == null)
                return false;

            _context.Spectacles.Remove(spectacle);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Business Methods

        public async Task<IEnumerable<Spectacle>> GetByCategoryAsync(SpectacleCategory category)
        {
            return await _context.Spectacles
                .Where(s => s.Category == category)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        //public async Task<IEnumerable<Spectacle>> GetByCreatorAsync(int userId)
        //{
        //    return await _context.Spectacles
        //        .Where(s => s.CreatedByUserId == userId)
        //        .OrderByDescending(s => s.CreatedAt)
        //        .ToListAsync();
        //}

        //public async Task<IEnumerable<Spectacle>> SearchByNameAsync(string searchTerm)
        //{
        //    return await _context.Spectacles
        //        .Where(s => EF.Functions.Like(s.Name, $"%{searchTerm}%"))   // Pattern SQL: cherche le term n'importe où dans le nom (insensible à la casse)
        //        .OrderBy(s => s.Name)
        //        .ToListAsync();
        //}

        public async Task<Spectacle?> GetWithPerformancesAsync(int id)
        {
            return await _context.Spectacles
                .Include(s => s.Performances)                       // Eager Loading (une seule requete au lieu de N+1 requetes)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(s => s.SpectacleId == id);
        }

        public async Task<int> CountByCategoryAsync(SpectacleCategory category)
        {
            return await _context.Spectacles
                .CountAsync(s => s.Category == category);
        }

        #endregion
    }
}
