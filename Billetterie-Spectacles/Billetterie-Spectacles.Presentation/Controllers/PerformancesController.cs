using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Billetterie_Spectacles.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerformancesController(IPerformanceService performanceService,ISpectacleService spectacleService) : ControllerBase
    {
        private readonly IPerformanceService _performanceService = performanceService;     // pour CRUD sur les représentations
        private readonly ISpectacleService _spectacleService = spectacleService;           // pour vérifier la permissions (org/admin)




        // Méthodes helper (similaires à SpectaclesController)
        #region Method Helper
        /// <summary>
        /// Récupère l'ID de l'utilisateur connecté depuis le token JWT
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Si le claim n'existe pas</exception>
        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("ID utilisateur introuvable dans le token JWT");
            }

            return int.Parse(userIdClaim);
        }

        /// <summary>
        /// Récupère le rôle de l'utilisateur connecté depuis le token JWT
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Si le claim n'existe pas</exception>
        private UserRole GetCurrentUserRole()
        {
            string? roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(roleClaim))
            {
                throw new UnauthorizedAccessException("Rôle utilisateur introuvable dans le token JWT");
            }

            return Enum.Parse<UserRole>(roleClaim);
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut modifier une performance
        /// (en vérifiant les droits sur le spectacle parent)
        /// </summary>
        private async Task<bool> CanModifyPerformance(int performanceId)
        {
            UserRole currentUserRole = GetCurrentUserRole();

            // Les admins peuvent tout modifier
            if (currentUserRole == UserRole.Admin)
                return true;

            // Les organisateurs ne peuvent modifier que les performances 
            // des spectacles qu'ils ont créés
            if (currentUserRole == UserRole.Organizer)
            {
                var performance = await _performanceService.GetByIdAsync(performanceId);
                if (performance == null)
                    return false;

                // Récupérer le spectacle parent pour vérifier le créateur
                var spectacle = await _spectacleService.GetByIdAsync(performance.SpectacleId);
                if (spectacle == null)
                    return false;

                var currentUserId = GetCurrentUserId();
                return spectacle.CreatedByUserId == currentUserId;
            }

            // Les clients ne peuvent rien modifier
            return false;
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut créer une performance 
        /// pour un spectacle donné
        /// </summary>
        private async Task<bool> CanCreatePerformanceForSpectacle(int spectacleId)
        {
            var currentUserRole = GetCurrentUserRole();

            // Les admins peuvent tout créer
            if (currentUserRole == UserRole.Admin)
                return true;

            // Les organisateurs ne peuvent créer des performances que 
            // pour leurs propres spectacles
            if (currentUserRole == UserRole.Organizer)
            {
                SpectacleDto? spectacle = await _spectacleService.GetByIdAsync(spectacleId);
                if (spectacle == null)
                    return false;

                int currentUserId = GetCurrentUserId();
                return spectacle.CreatedByUserId == currentUserId;
            }

            // Les clients ne peuvent rien créer
            return false;
        }
        #endregion


        #region Endpoints
        /// <summary>
        /// Récupère la liste de toutes les performances
        /// </summary>
        /// <returns>Liste des performances triées par date</returns>
        [HttpGet]
        [AllowAnonymous]    // Accessible sans être connecté
        [ProducesResponseType(typeof(IEnumerable<PerformanceDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PerformanceDto>>> GetAllPerformances()
        {
            try
            {
                IEnumerable<PerformanceDto> performances = await _performanceService.GetAllAsync();
                return Ok(performances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des performances", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les détails d'une performance par son ID
        /// </summary>
        /// <param name="performanceId">ID de la performance</param>
        /// <returns>Détails de la performance</returns>
        [HttpGet("{performanceId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PerformanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PerformanceDto>> GetPerformanceById(int performanceId)
        {
            try
            {
                PerformanceDto? performance = await _performanceService.GetByIdAsync(performanceId);

                if (performance == null)
                {
                    return NotFound(new { message = $"Aucune performance trouvée avec l'ID {performanceId}" });
                }

                return Ok(performance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération de la performance", error = ex.Message });
            }
        }

        /// <summary>
        /// Crée une nouvelle performance pour un spectacle
        /// </summary>
        /// <param name="spectacleId">ID du spectacle parent</param>
        /// <param name="dto">Données de la performance à créer</param>
        /// <returns>La performance créée</returns>
        [HttpPost("/api/spectacles/{spectacleId}/performances")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(typeof(PerformanceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PerformanceDto>> CreatePerformance(
            int spectacleId,                        // fourni par l'url
            [FromBody] CreatePerformanceDto dto)    // fourni par le body JSON
        {
            try
            {
                // Vérifier que le spectacle existe
                SpectacleDto? spectacle = await _spectacleService.GetByIdAsync(spectacleId);
                if (spectacle == null)
                {
                    return NotFound(new { message = $"Aucun spectacle trouvé avec l'ID {spectacleId}" });
                }

                // Vérifier les permissions : Admin ou propriétaire du spectacle
                if (!await CanCreatePerformanceForSpectacle(spectacleId))
                {
                    return Forbid(); // 403 Forbidden
                }

                // Valider que la date est dans le futur (doublon avec la validation au niveau du CreatePerformanceDto)
                if (dto.Date <= DateTime.Now)
                {
                    return BadRequest(new { message = "La date de la performance doit être dans le futur" });
                }

                // Créer la performance
                PerformanceDto createdPerformance = await _performanceService.CreatePerformanceAsync(spectacleId, dto);

                // Retourner 201 Created avec l'URL de la ressource créée
                return Created(
                    $"/api/performances/{createdPerformance.Id}",
                    createdPerformance
                );
            }
            catch (ArgumentException ex)
            {
                // Erreur de validation
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la création de la performance", error = ex.Message });
            }
        }

        /// <summary>
        /// Modifie une performance existante
        /// </summary>
        /// <param name="performanceId">ID de la performance à modifier</param>
        /// <param name="dto">Nouvelles données de la performance</param>
        /// <returns>La performance modifiée</returns>
        [HttpPut("{performanceId}")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(typeof(PerformanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PerformanceDto>> UpdatePerformance(
            int performanceId,
            [FromBody] UpdatePerformanceDto dto)
        {
            try
            {
                // Vérifier que la performance existe
                PerformanceDto? existingPerformance = await _performanceService.GetByIdAsync(performanceId);
                if (existingPerformance == null)
                {
                    return NotFound(new { message = $"Aucune performance trouvée avec l'ID {performanceId}" });
                }

                // Vérifier les permissions : Admin ou propriétaire du spectacle parent
                if (!await CanModifyPerformance(performanceId))
                {
                    return Forbid(); // 403 Forbidden
                }

                // La validation de date future est gérée par [FutureDate] sur le DTO


                // Effectuer la modification
                PerformanceDto updatedPerformance = await _performanceService.UpdatePerformanceAsync(performanceId, dto);

                return Ok(updatedPerformance);
            }
            catch (ArgumentException ex)
            {
                // Erreur de validation
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la modification de la performance", error = ex.Message });
            }
        }

        /// <summary>
        /// Supprime une performance
        /// </summary>
        /// <param name="performanceId">ID de la performance à supprimer</param>
        /// <returns>Aucun contenu</returns>
        [HttpDelete("{performanceId}")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePerformance(int performanceId)
        {
            try
            {
                // Vérifier que la performance existe
                PerformanceDto? existingPerformance = await _performanceService.GetByIdAsync(performanceId);
                if (existingPerformance == null)
                {
                    return NotFound(new { message = $"Aucune performance trouvée avec l'ID {performanceId}" });
                }

                // Vérifier les permissions : Admin ou propriétaire du spectacle parent
                if (!await CanModifyPerformance(performanceId))
                {
                    return Forbid(); // 403 Forbidden
                }

                // Effectuer la suppression
                await _performanceService.DeletePerformanceAsync(performanceId);

                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la suppression de la performance", error = ex.Message });
            }
        }
        #endregion
    }
}
