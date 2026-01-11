using Billeterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;

namespace Billetterie_Spectacles.Application.Services.Interfaces
{
    /// <summary>
    /// Service de gestion des spectacles
    /// Orchestre la logique métier liée aux spectacles
    /// </summary>
    public interface ISpectacleService
    {
        // Consultation
        Task<SpectacleDto?> GetByIdAsync(int spectacleId);
        Task<IEnumerable<SpectacleDto>> GetAllAsync();
        Task<IEnumerable<SpectacleDto>> GetByCategoryAsync(SpectacleCategory category);
        Task<IEnumerable<SpectacleDto>> SearchByNameAsync(string searchTerm);
        Task<SpectacleDto?> GetWithPerformancesAsync(int spectacleId);       // Eager Loading

        // Création et modification
        Task<SpectacleDto> CreateSpectacleAsync(CreateSpectacleDto dto, int createdByUserId);   // UserId fournit par Token JWT pour permission
        Task<SpectacleDto> UpdateSpectacleAsync(int spectacleId, UpdateSpectacleDto dto, int currentUserId, bool isAdmin);   // Crendentials fournis par Token JWT pour permission
        Task<bool> DeleteSpectacleAsync(int spectacleId, int currentUserId, bool isAdmin);   // Crendentials fournis par Token JWT pour permission

        // Statistiques
        Task<int> CountByCategoryAsync(SpectacleCategory category);
    }
}
