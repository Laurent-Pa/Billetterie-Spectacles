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
        Task<SpectacleDto?> GetWithPerformancesAsync(int spectacleId);       // Eager Loading
        Task<IEnumerable<SpectacleDto>> SearchAsync(string? name, SpectacleCategory? category, int? minDuration, int? maxDuration);  // Recherche multi-critères

        // Création et modification
        Task<SpectacleDto> CreateSpectacleAsync(CreateSpectacleDto dto, int createdByUserId);   // UserId fournit par Token JWT pour permission
        Task<SpectacleDto> UpdateSpectacleAsync(int spectacleId, UpdateSpectacleDto dto);   // La validation d'autorisation se fait dans le controller
        Task<bool> DeleteSpectacleAsync(int spectacleId);   // La validation d'autorisation se fait dans le controller

        // Statistiques
        Task<int> CountByCategoryAsync(SpectacleCategory category);
    }
}
