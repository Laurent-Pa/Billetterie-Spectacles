using Billeterie_Spectacles.Domain.Enums;
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
    public class SpectaclesController(ISpectacleService spectacleService) : ControllerBase
    {
        private readonly ISpectacleService _spectacleService = spectacleService;
    

     // Méthode helper pour récupérer l'ID de l'utilisateur connecté
        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim);
        }

        // Méthode helper pour récupérer le rôle de l'utilisateur connecté
        private UserRole GetCurrentUserRole()
        {
            string? roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.Parse<UserRole>(value: roleClaim);
        }

        // Méthode helper pour vérifier si l'utilisateur peut modifier/supprimer un spectacle
        private async Task<bool> CanModifySpectacle(int spectacleId)
        {
            UserRole currentUserRole = GetCurrentUserRole();

            // Les admins peuvent tout modifier
            if (currentUserRole == UserRole.Admin)
                return true;

            // Les organisateurs ne peuvent modifier que leurs propres spectacles
            if (currentUserRole == UserRole.Organizer)
            {
                SpectacleDto? spectacle = await _spectacleService.GetByIdAsync(spectacleId);
                if (spectacle == null)
                    return false;

                int currentUserId = GetCurrentUserId();
                return spectacle.CreatedByUserId == currentUserId;
            }

            // Les clients ne peuvent rien modifier
            return false;
        }

        // <summary>
        /// Récupère la liste de tous les spectacles
        /// </summary>
        /// <returns>Liste des spectacles</returns>
        [HttpGet]
        [AllowAnonymous]  // Accessible sans authentification
        [ProducesResponseType(typeof(IEnumerable<SpectacleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SpectacleDto>>> GetAllSpectacles()
        {
            try
            {
                IEnumerable<SpectacleDto> spectacles = await _spectacleService.GetAllAsync();
                return Ok(spectacles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des spectacles", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les détails d'un spectacle par son ID (sans les performances)
        /// </summary>
        /// <param name="spectacleId">ID du spectacle</param>
        /// <returns>Détails du spectacle</returns>
        [HttpGet("{spectacleId}")]
        [AllowAnonymous]  // Accessible sans authentification
        [ProducesResponseType(typeof(SpectacleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SpectacleDto>> GetSpectacleById(int spectacleId)
        {
            try
            {
                SpectacleDto? spectacle = await _spectacleService.GetByIdAsync(spectacleId);

                if (spectacle == null)
                {
                    return NotFound(new { message = $"Aucun spectacle trouvé avec l'ID {spectacleId}" });
                }

                return Ok(spectacle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération du spectacle", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère un spectacle par son ID AVEC toutes ses performances
        /// </summary>
        /// <param name="spectacleId">ID du spectacle</param>
        /// <returns>Détails du spectacle avec les représentations correspondantes</returns>
        [HttpGet("{spectacleId}/performances")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SpectacleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SpectacleDto>> GetSpectacleByIdWithPerformances(int spectacleId)
        {
            try
            {
                SpectacleDto? spectacle = await _spectacleService.GetWithPerformancesAsync(spectacleId);

                if (spectacle == null)
                {
                    return NotFound(new { message = $"Aucun spectacle trouvé avec l'ID {spectacleId}" });
                }

                return Ok(spectacle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        /// <summary>
        /// Crée un nouveau spectacle
        /// </summary>
        /// <param name="dto">Données du spectacle à créer</param>
        /// <returns>Le spectacle créé</returns>
        [HttpPost]
        [Authorize(Roles = "Organizer,Admin")]  // Uniquement Organizer et Admin
        [ProducesResponseType(typeof(SpectacleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<SpectacleDto>> CreateSpectacle([FromBody] CreateSpectacleDto dto)
        {
            try
            {
                // Récupérer l'ID de l'utilisateur connecté depuis le token JWT
                int currentUserId = GetCurrentUserId();

                // Créer le spectacle avec l'ID du créateur
                SpectacleDto createdSpectacle = await _spectacleService.CreateSpectacleAsync(dto, currentUserId);

                // Retourner 201 Created avec l'URL du nouveau spectacle créé et ses données (pour redirection Front)
                return Created(
                           $"/api/spectacles/{createdSpectacle.Id}",
                           createdSpectacle
                               );
            }
            catch (ArgumentException ex)
            {
                // En cas d'erreur de validation 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la création du spectacle", error = ex.Message });
            }
        }

        /// <summary>
        /// Modifie un spectacle existant
        /// </summary>
        /// <param name="spectacleId">ID du spectacle à modifier</param>
        /// <param name="dto">Nouvelles données du spectacle</param>
        /// <returns>Le spectacle modifié</returns>
        [HttpPut("{spectacleId}")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(typeof(SpectacleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SpectacleDto>> UpdateSpectacle(int spectacleId, [FromBody] UpdateSpectacleDto dto)
        {
            try
            {
                // Vérifier que le spectacle existe
                SpectacleDto? existingSpectacle = await _spectacleService.GetByIdAsync(spectacleId);
                if (existingSpectacle == null)
                {
                    return NotFound(new { message = $"Aucun spectacle trouvé avec l'ID {spectacleId}" });
                }

                // Vérifier les permissions : Admin ou propriétaire du spectacle
                if (!await CanModifySpectacle(spectacleId))
                {
                    return Forbid(); // 403 Forbidden
                }

                // Effectuer la modification
                SpectacleDto? updatedSpectacle = await _spectacleService.UpdateSpectacleAsync(spectacleId, dto);

                return Ok(updatedSpectacle);
            }
            catch (ArgumentException ex)
            {
                // Erreur de validation (ex: catégorie invalide)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la modification du spectacle", error = ex.Message });
            }
        }

        /// <summary>
        /// Modifie un spectacle existant
        /// </summary>
        /// <param name="spectacleId">ID du spectacle à modifier</param>
        /// <param name="dto">Nouvelles données du spectacle</param>
        /// <returns>Le spectacle modifié</returns>
        [HttpDelete("{spectacleId}")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]         // ASP.NET convertit la saisie en int : renvoi 400 si la conversion échoue (saisie d'une string)
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DeleteSpectacle(int spectacleId)
        {
            try
            {
                // Vérifier que le spectacle existe
                SpectacleDto? existingSpectacle = await _spectacleService.GetByIdAsync(spectacleId);
                if (existingSpectacle == null)
                {
                    return NotFound(new { message = $"Aucun spectacle trouvé avec l'ID {spectacleId}" });
                }

                // Vérifier les permissions : Admin ou propriétaire du spectacle
                if (!await CanModifySpectacle(spectacleId))
                {
                    return Forbid(); // 403 Forbidden
                }

                // Effectuer la suppression
                 await _spectacleService.DeleteSpectacleAsync(spectacleId);

                return NoContent(); // 204 No Content

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la suppression du spectacle", error = ex.Message });
            }
        }

        /// <summary>
        /// Recherche des spectacles avec filtres optionnels
        /// </summary>
        /// <param name="name">Recherche partielle dans le nom du spectacle</param>
        /// <param name="category">Filtre par catégorie (Theatre, Concert, Danse)</param>
        /// <param name="minDuration">Durée minimale en minutes</param>
        /// <param name="maxDuration">Durée maximale en minutes</param>
        /// <returns>Liste des spectacles correspondant aux critères</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<SpectacleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<SpectacleDto>>> SearchSpectacles(
            [FromQuery] string? name = null,
            [FromQuery] string? category = null,
            [FromQuery] int? minDuration = null,
            [FromQuery] int? maxDuration = null)
        {
            try
            {
                // Valider les paramètres
                if (minDuration.HasValue && minDuration.Value < 0)
                {
                    return BadRequest(new { message = "La durée minimale ne peut pas être négative" });
                }

                if (maxDuration.HasValue && maxDuration.Value < 0)
                {
                    return BadRequest(new { message = "La durée maximale ne peut pas être négative" });
                }

                if (minDuration.HasValue && maxDuration.HasValue && minDuration.Value > maxDuration.Value)
                {
                    return BadRequest(new { message = "La durée minimale ne peut pas être supérieure à la durée maximale" });
                }

                // Valider la catégorie si fournie
                SpectacleCategory? categoryEnum = null;
                if (!string.IsNullOrWhiteSpace(category))
                {
                    if (!Enum.TryParse<SpectacleCategory>(category, ignoreCase: true, out SpectacleCategory parsedCategory))
                    {
                        return BadRequest(new { message = $"Catégorie invalide : {category}. Valeurs acceptées : Theatre, Concert, Danse" });
                    }
                    categoryEnum = parsedCategory;
                }

                // Appeler le service de recherche
                var spectacles = await _spectacleService.SearchAsync(name, categoryEnum, minDuration, maxDuration);

                return Ok(spectacles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la recherche", error = ex.Message });
            }
        }
    }
}
