using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Domain.Enums;

namespace Billetterie_Spectacles.Application.Services.Interfaces
{
    /// <summary>
    /// Service de gestion des commandes
    /// Orchestre la logique métier complexe liée aux commandes et tickets
    /// </summary>
    public interface IOrderService
    {
        // Consultation
        Task<OrderDto?> GetByIdAsync(int orderId);
        Task<IEnumerable<OrderDto>> GetAllAsync();                  // Pour Admin 
        Task<IEnumerable<OrderDto>> GetAllWithFiltersAsync(int? userId = null, int? performanceId = null,OrderStatus? orderStatus = null); // Pour l'organisateur et admin?
        Task<IEnumerable<OrderDto>> GetByUserIdAsync(int userId);   // Pour le client
        Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatus status);
        Task<OrderDto?> GetWithTicketsAsync(int orderId);

        // Création et gestion
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, int userId);
        Task<OrderDto> CancelOrderAsync(int orderId, int currentUserId);
        Task<OrderDto> ChangeOrderStatusAsync(int orderId, OrderStatus newStatus, int currentUserId, bool isAdmin);

        // Statistiques
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetTotalRevenueByUserAsync(int userId);
        Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count = 10);
    }
}