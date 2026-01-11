using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Application.Mappings;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Exceptions;

namespace Billetterie_Spectacles.Application.Services.Implementations
{
    /// <summary>
    /// Implémentation du service de gestion des performances
    /// </summary>
    public class PerformanceService(
        IPerformanceRepository _performanceRepository,              // Pour le CRUD des représentations
        ISpectacleRepository _spectacleRepository,                  // Pour vérifier les permissions sur spectacle parent
        ITicketRepository _ticketRepository) : IPerformanceService  // Accès au décompte des billets vendus pour validation
    {
        #region Consultation

        public async Task<PerformanceDto?> GetByIdAsync(int performanceId)
        {
            Performance? performance = await _performanceRepository.GetByIdAsync(performanceId);
            return performance != null ? PerformanceMapper.EntityToDto(performance) : null;
        }

        public async Task<IEnumerable<PerformanceDto>> GetAllAsync()
        {
            IEnumerable<Performance> performances = await _performanceRepository.GetAllAsync();
            return performances.Select(PerformanceMapper.EntityToDto);
        }

        public async Task<IEnumerable<PerformanceDto>> GetBySpectacleIdAsync(int spectacleId)
        {
            IEnumerable<Performance> performances = await _performanceRepository.GetBySpectacleIdAsync(spectacleId);
            return performances.Select(PerformanceMapper.EntityToDto);
        }

        public async Task<IEnumerable<PerformanceDto>> GetByStatusAsync(PerformanceStatus status)
        {
            IEnumerable<Performance> performances = await _performanceRepository.GetByStatusAsync(status);
            return performances.Select(PerformanceMapper.EntityToDto);
        }

        public async Task<IEnumerable<PerformanceDto>> GetUpcomingAsync()
        {
            IEnumerable<Performance> performances = await _performanceRepository.GetUpcomingAsync();
            return performances.Select(PerformanceMapper.EntityToDto);
        }

        public async Task<IEnumerable<PerformanceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("La date de début doit être antérieure à la date de fin.");
            }

            IEnumerable<Performance> performances = await _performanceRepository.GetByDateRangeAsync(startDate, endDate);
            return performances.Select(PerformanceMapper.EntityToDto);
        }

        public async Task<PerformanceDto?> GetWithTicketsAsync(int performanceId)
        {
            Performance? performance = await _performanceRepository.GetWithTicketsAsync(performanceId);
            return performance != null ? PerformanceMapper.EntityToDto(performance) : null;
        }


        // Disponibilité
        public async Task<bool> HasAvailableTicketsAsync(int performanceId)
        {
            return await _performanceRepository.HasAvailableTicketsAsync(performanceId);
        }

        public async Task<int> GetAvailableTicketsCountAsync(int performanceId)
        {
            return await _performanceRepository.GetAvailableTicketsCountAsync(performanceId);
        }

        #endregion

        #region Création et modification

        public async Task<PerformanceDto> CreatePerformanceAsync(CreatePerformanceDto dto, int currentUserId, bool isAdmin)
        {
            // Vérifier que le spectacle existe
            Spectacle? spectacle = await _spectacleRepository.GetByIdAsync(dto.SpectacleId) 
                ?? throw new NotFoundException($"Spectacle avec l'ID {dto.SpectacleId} introuvable.");

            // Vérifier les permissions via le spectacle parent
            if (!isAdmin && spectacle.CreatedByUserId != currentUserId)
            {
                throw new ForbiddenException("Vous n'avez pas la permission de créer des performances pour ce spectacle.");
            }

            // Créer l'entité via le mapper
            Performance performance = PerformanceMapper.CreateDtoToEntity(dto);

            // Sauvegarder
            Performance? createdPerformance = await _performanceRepository.AddAsync(performance);

            return createdPerformance == null
                ? throw new DomainException("Erreur lors de la création de la performance.")
                : PerformanceMapper.EntityToDto(createdPerformance);
        }

        public async Task<PerformanceDto> UpdatePerformanceAsync(int performanceId, UpdatePerformanceDto dto, int currentUserId, bool isAdmin)
        {
            // Récupérer la performance
            Performance? performance = await _performanceRepository.GetByIdAsync(performanceId) 
                ?? throw new NotFoundException($"Performance avec l'ID {performanceId} introuvable.");

            // Vérifier les permissions via le spectacle parent
            Spectacle? spectacle = await _spectacleRepository.GetByIdAsync(performance.SpectacleId) ?? throw new NotFoundException($"Spectacle avec l'ID {performance.SpectacleId} introuvable.");
            if (!isAdmin && spectacle.CreatedByUserId != currentUserId)
            {
                throw new ForbiddenException("Vous n'avez pas la permission de modifier cette performance.");
            }

            // Si on modifie la capacité, vérifier qu'on ne descend pas sous les billets vendus
            int ticketsSold = await _ticketRepository.CountByPerformanceIdAsync(performanceId);
            if (dto.Capacity < ticketsSold)
            {
                throw new DomainException(
                    $"Impossible de réduire la capacité à {dto.Capacity}. " +
                    $"{ticketsSold} billets ont déjà été vendus."
                );
            }

            // Mettre à jour via le mapper
            PerformanceMapper.UpdateEntity(performance, dto);

            // Sauvegarder
            Performance updatedPerformance = await _performanceRepository.UpdateAsync(performance);

            return PerformanceMapper.EntityToDto(updatedPerformance);
        }

        public async Task<PerformanceDto> CancelPerformanceAsync(int performanceId, int currentUserId, bool isAdmin)
        {
            // Récupérer la performance
            Performance? performance = await _performanceRepository.GetByIdAsync(performanceId) 
                ?? throw new NotFoundException($"Performance avec l'ID {performanceId} introuvable.");

            // Vérifier les permissions via le spectacle parent
            Spectacle? spectacle = await _spectacleRepository.GetByIdAsync(performance.SpectacleId) 
                ?? throw new NotFoundException($"Spectacle avec l'ID {performance.SpectacleId} introuvable.");
            if (!isAdmin && spectacle.CreatedByUserId != currentUserId)
            {
                throw new ForbiddenException("Vous n'avez pas la permission d'annuler cette performance.");
            }

            // Vérifier que la performance n'est pas déjà annulée ou terminée
            if (performance.Status == PerformanceStatus.Cancelled)
            {
                throw new DomainException("Cette performance est déjà annulée.");
            }

            if (performance.Status == PerformanceStatus.Completed)
            {
                throw new DomainException("Impossible d'annuler une performance déjà terminée.");
            }

            // Annuler la performance (change le status)
            performance.Cancel();

            // Sauvegarder
            Performance cancelledPerformance = await _performanceRepository.UpdateAsync(performance);

            // TODO Plus tard : 
            // - Récupérer tous les tickets de cette performance
            // - Marquer les tickets comme Cancelled
            // - Changer les commandes en Refunded
            // - Notifier tous les acheteurs par email
            // - Gérer les remboursements

            return PerformanceMapper.EntityToDto(cancelledPerformance);
        }

        public async Task<bool> DeletePerformanceAsync(int performanceId, int currentUserId, bool isAdmin)
        {
            // Récupérer la performance
            Performance? performance = await _performanceRepository.GetByIdAsync(performanceId);
            if (performance == null)
            {
                return false;  // Performance n'existe pas
            }

            // Vérifier les permissions via le spectacle parent
            Spectacle? spectacle = await _spectacleRepository.GetByIdAsync(performance.SpectacleId) 
                ?? throw new NotFoundException($"Spectacle avec l'ID {performance.SpectacleId} introuvable.");

            if (!isAdmin && spectacle.CreatedByUserId != currentUserId)
            {
                throw new ForbiddenException("Vous n'avez pas la permission de supprimer cette performance.");
            }

            // Vérifier qu'il n'y a pas de billets vendus
            int ticketsSold = await _ticketRepository.CountByPerformanceIdAsync(performanceId);
            if (ticketsSold > 0)
            {
                throw new DomainException(
                    $"Impossible de supprimer cette performance. {ticketsSold} billets ont été vendus. " +
                    "Veuillez annuler la performance au lieu de la supprimer."
                );
            }

            // Supprimer
            return await _performanceRepository.DeleteAsync(performanceId);
        }

        #endregion
    }
}
