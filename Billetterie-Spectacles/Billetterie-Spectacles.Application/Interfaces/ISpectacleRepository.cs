using Billeterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Entities;

namespace Billetterie_Spectacles.Application.Interfaces
{
    /// <summary>
    /// Contrat définissant les opérations de persistance pour l'entité Spectacle
    /// C'est une interface, elle définit quoi faire (les fonctionnalités).
    /// C'est son implémentation qui définit le "comment faire". 
    /// </summary>
    public interface ISpectacleRepository
    {
        // Opérations CRUD de base
        Task<Spectacle> AddAsync(Spectacle spectacle);      // Create
        Task<Spectacle?> GetByIdAsync(int id);              // Read
        Task<IEnumerable<Spectacle>> GetAllAsync();         // Read
        Task<Spectacle> UpdateAsync(Spectacle spectacle);   // Update
        Task<bool> DeleteAsync(int id);                     // Delete

        // Méthodes métier spécifiques
        Task<IEnumerable<Spectacle>> GetByCategoryAsync(SpectacleCategory category);    // Pour utilisateur : filtrer l'affichage des spectacles
        Task<IEnumerable<Spectacle>> GetByCreatorAsync(int userId);                     // Pour administration : afficher uniquement ses spectacles
        Task<IEnumerable<Spectacle>> SearchByNameAsync(string searchTerm);              // Pour utilisateur : barre de recherche (penser à une requete SQL pour recherche partielle)
        Task<Spectacle?> GetWithPerformancesAsync(int id);                              // Pour utilisateur (Eager loading): afficher toutes les dates dispo d'un spectacle spécifique
        Task<int> CountByCategoryAsync(SpectacleCategory category);                     // Pour administration : statistiques par categorie
    }
}
