using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;

namespace Billetterie_Spectacles.Application.Interfaces
{
    /// <summary>
    /// Contrat définissant les opérations de persistance pour l'entité Ticket
    /// </summary>
    public interface ITicketRepository
    {
        // Opérations CRUD de base
        Task<Ticket?> AddAsync(Ticket ticket);      // Create
        Task<Ticket?> GetByIdAsync(int id);         // Read
        Task<IEnumerable<Ticket?>> GetAllAsync();    // Read
        Task<Ticket> UpdateAsync(Ticket ticket);    // Update
        Task<bool> DeleteAsync(int id);             // Delete

        // Méthodes métier spécifiques
        Task<IEnumerable<Ticket>> GetByOrderIdAsync(int orderId);               // Pour utilisateur : afficher tous les billets d'une commande spécifique
        Task<IEnumerable<Ticket>> GetByOrderWithPerformanceAsync(int orderId);  // Pour utilisateur : affiche les détails du spectacle correspondant au billet (date, nom, categorie)
        Task<IEnumerable<Ticket>> GetByPerformanceIdAsync(int performanceId);   // Pour administration : savoir quels billets ont été vendus pour une représentation
        Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status);        // Pour administration : gestion du cycle de vie des billets
        Task<int> CountByPerformanceIdAsync(int performanceId);                 // Pour administration : compte les billets pour une représentation (et vérif croisée avec Performance.AvailableTickets)
    }
}
