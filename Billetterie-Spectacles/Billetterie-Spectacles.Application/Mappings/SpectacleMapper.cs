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
            return new SpectacleDto(
                Id: spectacle.SpectacleId,      // SpectacleId simplifié pour apppel API
                Name: spectacle.Name,
                Category: spectacle.Category.ToString(),  // Enum → String
                Description: spectacle.Description,
                Duration: spectacle.Duration,
                Thumbnail: spectacle.Thumbnail,
                CreatedAt: spectacle.CreatedAt,
                UpdatedAt: spectacle.UpdatedAt
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
