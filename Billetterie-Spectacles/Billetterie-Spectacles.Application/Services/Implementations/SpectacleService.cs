using Billeterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Application.Mappings;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Exceptions;

namespace Billetterie_Spectacles.Application.Services.Implementations
{
    /// <summary>
    /// Implémentation du service de gestion des spectacles
    /// </summary>
    public class SpectacleService(ISpectacleRepository _spectacleRepository) : ISpectacleService
    {
        #region Consultation

        public async Task<SpectacleDto?> GetByIdAsync(int spectacleId)
        {
            Spectacle? spectacle = await _spectacleRepository.GetByIdAsync(spectacleId);
            return spectacle != null ? SpectacleMapper.EntityToDto(spectacle) : null;
        }

        public async Task<IEnumerable<SpectacleDto>> GetAllAsync()
        {
            IEnumerable<Spectacle> spectacles = await _spectacleRepository.GetAllAsync();
            return spectacles.Select(SpectacleMapper.EntityToDto);
        }

        public async Task<SpectacleDto?> GetWithPerformancesAsync(int spectacleId)
        {
            Spectacle? spectacle = await _spectacleRepository.GetWithPerformancesAsync(spectacleId);
            return spectacle != null ? SpectacleMapper.EntityToDtoWithPerformances(spectacle) : null;
        }

        public async Task<int> CountByCategoryAsync(SpectacleCategory category)
        {
            return await _spectacleRepository.CountByCategoryAsync(category);
        }

        public async Task<IEnumerable<SpectacleDto>> SearchAsync(string? name, SpectacleCategory? category, int? minDuration, int? maxDuration)
        {
            // Récupérer tous les spectacles depuis le repository
            IEnumerable<Spectacle> spectacles = await _spectacleRepository.GetAllAsync();

            // Appliquer les filtres un par un
            IEnumerable<Spectacle> filtered = spectacles;

            // Filtre par nom (recherche partielle, insensible à la casse)
            if (!string.IsNullOrWhiteSpace(name))
            {
                filtered = filtered.Where(s =>
                    s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            // Filtre par catégorie
            if (category.HasValue)
            {
                filtered = filtered.Where(s => s.Category == category.Value);
            }

            // Filtre par durée minimale
            if (minDuration.HasValue)
            {
                filtered = filtered.Where(s => s.Duration >= minDuration.Value);
            }

            // Filtre par durée maximale
            if (maxDuration.HasValue)
            {
                filtered = filtered.Where(s => s.Duration <= maxDuration.Value);
            }

            // Convertir en DTOs
            return filtered.Select(s => SpectacleMapper.EntityToDto(s));
        }

        #endregion

        #region Création et modification

        public async Task<SpectacleDto> CreateSpectacleAsync(CreateSpectacleDto dto, int createdByUserId)
        {
            // Validation : userId doit être positif        [TODO] meilleur validation en vérifiant que l'id existe en base
            if (createdByUserId <= 0)
            {
                throw new ArgumentException("L'ID de l'utilisateur créateur doit être valide.", nameof(createdByUserId));
            }

            // Créer l'entité via le mapper
            Spectacle spectacle = SpectacleMapper.CreateDtoToEntity(dto, createdByUserId);

            // Sauvegarder en base
            Spectacle? createdSpectacle = await _spectacleRepository.AddAsync(spectacle);

            return createdSpectacle == null
                ? throw new DomainException("Erreur lors de la création du spectacle.")
                : SpectacleMapper.EntityToDto(createdSpectacle);
        }

        public async Task<SpectacleDto> UpdateSpectacleAsync(int spectacleId, UpdateSpectacleDto dto)
        {
            // Récupérer le spectacle
            Spectacle? spectacle = await _spectacleRepository.GetByIdAsync(spectacleId)
                ?? throw new NotFoundException($"Spectacle avec l'ID {spectacleId} introuvable.");

            // Mettre à jour via le mapper
            SpectacleMapper.UpdateEntity(spectacle, dto);

            // Sauvegarder
            Spectacle updatedSpectacle = await _spectacleRepository.UpdateAsync(spectacle);

            return SpectacleMapper.EntityToDto(updatedSpectacle);
        }

        public async Task<bool> DeleteSpectacleAsync(int spectacleId)
        {
            // Récupérer le spectacle
            Spectacle? spectacle = await _spectacleRepository.GetByIdAsync(spectacleId);
            if (spectacle == null)
            {
                return false;  // Spectacle n'existe pas
            }

            // Supprimer
            return await _spectacleRepository.DeleteAsync(spectacleId);
        }

        #endregion
    }
}
