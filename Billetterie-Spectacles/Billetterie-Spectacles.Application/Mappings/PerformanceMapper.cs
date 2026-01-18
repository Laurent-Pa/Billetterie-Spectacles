using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;


namespace Billetterie_Spectacles.Application.Mappings
{
    /// <summary>
    /// Mapper pour les conversions entre entité Performance et ses DTOs
    /// </summary>
    public static class PerformanceMapper
    {
        /// <summary>
        /// Convertit une entité Performance en PerformanceDto
        /// </summary>
        public static PerformanceDto EntityToDto(Performance performance)
        {

            // Mapper le spectacle uniquement s'il est chargé
            // IMPORTANT : On utilise EntityToDtoWithoutPerformances pour éviter les références circulaires
            SpectacleDto? spectacle = null;
            if (performance.Spectacle != null)
            {
                spectacle = SpectacleMapper.EntityToDtoWithoutPerformances(performance.Spectacle);
            }

            return new PerformanceDto(
                Id: performance.PerformanceId,      // PerformanceId simplifié pour l'API
                Date: performance.Date,
                Status: performance.Status.ToString(),  // Enum → String
                Capacity: performance.Capacity,
                AvailableTickets: performance.AvailableTickets,
                UnitPrice: performance.UnitPrice,
                SpectacleId: performance.SpectacleId,
                Spectacle: spectacle
            );
        }

        /// <summary>
        /// Convertit un CreatePerformanceDto en entité Performance
        /// </summary>
        public static Performance CreateDtoToEntity(int spectacleId, CreatePerformanceDto dto)
        {
            return new Performance(
                spectacleId: spectacleId,   // l'ID du spectacle est fourni par l'URL de l'API, pas le DTO
                date: dto.Date,
                capacity: dto.Capacity,
                unitPrice: dto.UnitPrice
            );
        }

        /// <summary>
        /// Met à jour une entité Performance existante avec les données d'un UpdatePerformanceDto
        /// </summary>
        public static void UpdateEntity(Performance performance, UpdatePerformanceDto dto)
        {
            

            // Mettre à jour l'entité
            performance.UpdateDetails(
                date: dto.Date,
                capacity: dto.Capacity,
                unitPrice: dto.UnitPrice
            );
        }
    }
}
