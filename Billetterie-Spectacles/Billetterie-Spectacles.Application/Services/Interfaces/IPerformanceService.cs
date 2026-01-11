using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Domain.Enums;

namespace Billetterie_Spectacles.Application.Services.Interfaces
{
    /// <summary>
    /// Service de gestion des performances (représentations)
    /// Orchestre la logique métier liée aux performances
    /// </summary>
    public interface IPerformanceService
    {
        // Consultation
        Task<PerformanceDto?> GetByIdAsync(int performanceId);
        Task<IEnumerable<PerformanceDto>> GetAllAsync();
        Task<IEnumerable<PerformanceDto>> GetBySpectacleIdAsync(int spectacleId);
        Task<IEnumerable<PerformanceDto>> GetByStatusAsync(PerformanceStatus status);
        Task<IEnumerable<PerformanceDto>> GetUpcomingAsync();
        Task<IEnumerable<PerformanceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PerformanceDto?> GetWithTicketsAsync(int performanceId);

        // Création et modification
        Task<PerformanceDto> CreatePerformanceAsync(CreatePerformanceDto dto, int currentUserId, bool isAdmin);     // Permission gérée par le spectacle parent
        Task<PerformanceDto> UpdatePerformanceAsync(int performanceId, UpdatePerformanceDto dto, int currentUserId, bool isAdmin);
        Task<PerformanceDto> CancelPerformanceAsync(int performanceId, int currentUserId, bool isAdmin);            // Action spécifique différente d'un update car impact les tickets vendus
        Task<bool> DeletePerformanceAsync(int performanceId, int currentUserId, bool isAdmin);

        // Disponibilité
        Task<bool> HasAvailableTicketsAsync(int performanceId);
        Task<int> GetAvailableTicketsCountAsync(int performanceId);
    }
}
