using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Billetterie_Spectacles.Infrastructure.Repositories
{
    /// <summary>
    /// Implémentation du repository User avec Entity Framework Core
    /// Injection de dépendance avec variable privée _context
    /// </summary>
    public class UserRepository(BilletterieDbContext _context) : IUserRepository
    {

        #region CRUD Operations

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<IEnumerable<User?>> GetAllAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<User?> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            User? user = await GetByIdAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Business Methods

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<int> CountByRoleAsync(UserRole role)
        {
            return await _context.Users
                .CountAsync(u => u.Role == role);
        }

        #endregion
    }
}
