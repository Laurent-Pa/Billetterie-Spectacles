using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;

namespace Billetterie_Spectacles.Application.Interfaces
{
    /// <summary>
    /// Contrat définissant les opérations de persistance pour l'entité Order
    /// </summary>
    public interface IOrderRepository
    {
        // Opérations CRUD de base
        Task<Order> AddAsync(Order order);      // Create
        Task<Order?> GetByIdAsync(int id);      // Read
        Task<IEnumerable<Order>> GetAllAsync(); // Read
        Task<Order> UpdateAsync(Order order);   // Update
        Task<bool> DeleteAsync(int id);         // Delete

        // Méthodes métier spécifiques
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);                                  // Pour utilisateur : consulter son historique de commandes
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);                          // Pour gestion des commandes en cours et identifier les pb avec service
        Task<Order?> GetWithTicketsAsync(int id);                                               // Pour utilisateur (Eager loading): 1 requete pour récupérer ts les billets d'une commande
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);     // Pour administration : statistiques des commandes par période (mois, semaine,...)
        Task<decimal> GetTotalRevenueAsync();                                                   // Pour administration : statistiques des ventes (penser à filtrer en fonction du status)
        Task<decimal> GetTotalRevenueByUserAsync(int userId);                                   // Pour administration et utilisateurs : statistiques, programme de fidélité, segmentation client
        Task<int> CountByStatusAsync(OrderStatus status);                                       // Pour administration et gestion : nombre de commandes en attentes / annulées
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);                          // Pour administration : statistiques sur les 10 dernières commandes (ex retour d'impact commercial)
    }
}
