using Billeterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Domain.Entities;

namespace Billetterie_Spectacles.Application.Mappings
{
    /// <summary>
    /// Mapper pour les conversions entre entité Spectacle et ses DTOs
    /// </summary>
    public static class SpectacleMapper
    {
        /// <summary>
        /// Convertit une entité Spectacle en SpectacleDto
        /// </summary>
        public static SpectacleDto EntityToDto(Spectacle spectacle)
        {
            // Mapper les performances uniquement si elles sont chargées
            IEnumerable<PerformanceDto>? performances = null;
            if (spectacle.Performances != null && spectacle.Performances.Any())
            {
                performances = spectacle.Performances.Select(EntityToDtoWithoutSpectacle);
            }

            return new SpectacleDto(
                Id: spectacle.SpectacleId,      // SpectacleId simplifié pour apppel API
                Name: spectacle.Name,
                Category: spectacle.Category.ToString(),  // Enum → String
                Description: spectacle.Description,
                Duration: spectacle.Duration,
                Thumbnail: spectacle.Thumbnail,
                CreatedByUserId: spectacle.CreatedByUserId,
                CreatedAt: spectacle.CreatedAt,
                UpdatedAt: spectacle.UpdatedAt,
                Performances: null              // Optionnel : on envoi les représentations à la demande (API)
            );
        }

        /// <summary>
        /// Convertit une entité Spectacle en SpectacleDto SANS mapper les Performances
        /// Utilisé pour éviter les références circulaires quand on mappe depuis Performance
        /// </summary>
        public static SpectacleDto EntityToDtoWithoutPerformances(Spectacle spectacle)
        {
            return new SpectacleDto(
                Id: spectacle.SpectacleId,
                Name: spectacle.Name,
                Category: spectacle.Category.ToString(),
                Description: spectacle.Description,
                Duration: spectacle.Duration,
                Thumbnail: spectacle.Thumbnail,
                CreatedByUserId: spectacle.CreatedByUserId,
                CreatedAt: spectacle.CreatedAt,
                UpdatedAt: spectacle.UpdatedAt,
                Performances: null  // Pas de performances pour éviter la boucle
            );
        }

        /// <summary>
        /// Convertit une entité Spectacle en SpectacleDto avec ses performances
        /// </summary>
        public static SpectacleDto EntityToDtoWithPerformances(Spectacle spectacle)
        {
            return new SpectacleDto(
                Id: spectacle.SpectacleId,
                Name: spectacle.Name,
                Category: spectacle.Category.ToString(),
                Description: spectacle.Description,
                Duration: spectacle.Duration,
                Thumbnail: spectacle.Thumbnail,
                CreatedByUserId: spectacle.CreatedByUserId,
                CreatedAt: spectacle.CreatedAt,
                UpdatedAt: spectacle.UpdatedAt,
                Performances: spectacle.Performances?
                    .Select(p => PerformanceMapper.EntityToDto(p))  //  Mapper pour chaque performance
                    .ToList()
            );
        }

        /// <summary>
        /// Convertit une Performance en PerformanceDto SANS mapper le Spectacle
        /// Utilisé pour éviter les références circulaires quand on mappe depuis Spectacle
        /// </summary>
        private static PerformanceDto EntityToDtoWithoutSpectacle(Performance performance)
        {
            return new PerformanceDto(
                Id: performance.PerformanceId,
                Date: performance.Date,
                Status: performance.Status.ToString(),
                Capacity: performance.Capacity,
                UnitPrice: performance.UnitPrice,
                AvailableTickets: performance.AvailableTickets,
                SpectacleId: performance.SpectacleId,
                Spectacle: null  // Pas de spectacle pour éviter la boucle
            );
        }

        /// <summary>
        /// Convertit un CreateSpectacleDto en entité Spectacle
        /// </summary>
        /// <param name="dto">DTO de création</param>
        /// <param name="createdByUserId">ID de l'organisateur qui crée le spectacle</param>
        public static Spectacle CreateDtoToEntity(CreateSpectacleDto dto, int createdByUserId)
        {
            // Parser la catégorie (string → enum)
            if (!Enum.TryParse<SpectacleCategory>(dto.Category, ignoreCase: true, out SpectacleCategory category))
            {
                throw new ArgumentException($"Catégorie invalide : {dto.Category}");
            }

            return new Spectacle(
                name: dto.Name,
                category: category,
                description: dto.Description,
                duration: dto.Duration,
                thumbnail: dto.Thumbnail,
                createdByUserId: createdByUserId
            );
        }

        /// <summary>
        /// Met à jour une entité Spectacle existante avec les données d'un UpdateSpectacleDto
        /// </summary>
        public static void UpdateEntity(Spectacle spectacle, UpdateSpectacleDto dto)
        {
            // Parser la catégorie (string → enum)
            if (!Enum.TryParse<SpectacleCategory>(dto.Category, ignoreCase: true, out SpectacleCategory category))
            {
                throw new ArgumentException($"Catégorie invalide : {dto.Category}");
            }

            // Mettre à jour l'entité
            spectacle.UpdateDetails(
                name: dto.Name,
                category: category,
                description: dto.Description,
                duration: dto.Duration,
                thumbnail: dto.Thumbnail
            );
        }
    }
}
