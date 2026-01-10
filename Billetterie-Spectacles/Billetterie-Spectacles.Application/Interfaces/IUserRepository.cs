using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;

namespace Billetterie_Spectacles.Application.Interfaces
{
    /// <summary>
    /// Contrat définissant les opérations de persistance pour l'entité User.
    /// C'est une interface, elle définit quoi faire (les fonctionnalités).
    /// C'est son implémentation qui définit le "comment faire". 
    /// </summary>
    public interface IUserRepository
    {
        // Operations du CRUD
        Task<User?> AddAsync(User user);      // Create
        Task<User> GetByIdAsync(int id);        // Read
        Task<IEnumerable<User>> GetAllAsync();  // Read
        Task<User> UpdateAsync(User user);      // Update
        Task<bool> DeleteAsync(int id);         // Delete

        // Methodes métier spécifiques
        Task<User?> GetByEmailAsync(string email);  // Pour la connexion, on recherche par email pas par id
        Task<bool> EmailExistsAsync(string email);  // Pour valider en amont l'unicité d'un email à l'inscription
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);  // Pour faciliter l'administration
        Task<int> CountByRoleAsync(UserRole role);  // Pour faciliter l'export de statistiques
         
    }
}
