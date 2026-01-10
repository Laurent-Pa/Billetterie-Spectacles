using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;

namespace Billetterie_Spectacles.Application.Interfaces
{
    // <summary>
    /// Contrat définissant les opérations de persistance pour l'entité Performance
    /// </summary>
    public interface IPerformanceRepository
    {
        // Opérations CRUD de base
        Task<Performance> AddAsync(Performance performance);        // Create
        Task<Performance?> GetByIdAsync(int id);                    // Read
        Task<IEnumerable<Performance>> GetAllAsync();               // Read
        Task<Performance> UpdateAsync(Performance performance);     // Update
        Task<bool> DeleteAsync(int id);                             // Delete

        // Méthodes métier spécifiques
        Task<IEnumerable<Performance>> GetBySpectacleIdAsync(int spectacleId);                      // Pour utilisateur: afficher toutes les dates d'un spectacle
        Task<IEnumerable<Performance>> GetByStatusAsync(PerformanceStatus status);                  // Pour administration: filtrer en fonction du status
        Task<IEnumerable<Performance>> GetUpcomingAsync();                                          // Pour utilisateur: affiche spécifiquement les prochains spectacles (A l'affiche..)
        Task<IEnumerable<Performance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);   // Pour utilisateur : filtrer par date
        Task<Performance?> GetWithTicketsAsync(int id);                                             // Pour administration (Eager loading): statistiques des billets vendus d'une représentation
        Task<bool> HasAvailableTicketsAsync(int performanceId);                                     // Pour utilisateur : filtre des spectacles avec des billets disponibles
        Task<int> GetAvailableTicketsCountAsync(int performanceId);                                 // Pour utilisateur : affiche disponibilité sans charger toute l'entité
    }
}
