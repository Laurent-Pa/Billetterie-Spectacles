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

        public async Task<IEnumerable<SpectacleDto>> GetByCategoryAsync(SpectacleCategory category)
        {
            IEnumerable<Spectacle> spectacles = await _spectacleRepository.GetByCategoryAsync(category);
            return spectacles.Select(SpectacleMapper.EntityToDto);
        }

        public async Task<IEnumerable<SpectacleDto>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Le terme de recherche ne peut pas être vide.", nameof(searchTerm));
            }

            IEnumerable<Spectacle> spectacles = await _spectacleRepository.SearchByNameAsync(searchTerm);
            return spectacles.Select(SpectacleMapper.EntityToDto);
        }

        public async Task<SpectacleDto?> GetWithPerformancesAsync(int spectacleId)
        {
            Spectacle? spectacle = await _spectacleRepository.GetWithPerformancesAsync(spectacleId);
            return spectacle != null ? SpectacleMapper.EntityToDto(spectacle) : null;
        }

        public async Task<int> CountByCategoryAsync(SpectacleCategory category)
        {
            return await _spectacleRepository.CountByCategoryAsync(category);
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

        public async Task<SpectacleDto> UpdateSpectacleAsync(int spectacleId, UpdateSpectacleDto dto, int currentUserId, bool isAdmin)
        {
            // Récupérer le spectacle
            Spectacle? spectacle = await _spectacleRepository.GetByIdAsync(spectacleId) 
                ?? throw new NotFoundException($"Spectacle avec l'ID {spectacleId} introuvable.");

            // Vérification des permissions
            if (!isAdmin && spectacle.CreatedByUserId != currentUserId)
            {
                throw new ForbiddenException("Vous n'avez pas la permission de modifier ce spectacle.");
            }

            // Mettre à jour via le mapper
            SpectacleMapper.UpdateEntity(spectacle, dto);

            // Sauvegarder
            Spectacle updatedSpectacle = await _spectacleRepository.UpdateAsync(spectacle);

            return SpectacleMapper.EntityToDto(updatedSpectacle);
        }

        public async Task<bool> DeleteSpectacleAsync(int spectacleId, int currentUserId, bool isAdmin)
        {
            // Récupérer le spectacle
            Spectacle? spectacle = await _spectacleRepository.GetByIdAsync(spectacleId);
            if (spectacle == null)
            {
                return false;  // Spectacle n'existe pas
            }

            // Vérification des permissions
            if (!isAdmin && spectacle.CreatedByUserId != currentUserId)
            {
                throw new ForbiddenException("Vous n'avez pas la permission de supprimer ce spectacle.");
            }

            // Supprimer
            return await _spectacleRepository.DeleteAsync(spectacleId);
        }

        #endregion
    }
}
