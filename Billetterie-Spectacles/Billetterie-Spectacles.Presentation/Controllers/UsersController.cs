using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.DTO.UserSensitive;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Billetterie_Spectacles.Presentation.Controllers
{
    /// <summary>
    /// Contrôleur de gestion des utilisateurs
    /// Toutes les routes nécessitent une authentification
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Toutes les routes de cette classe nécessitent un toket JWT
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Récupérer tous les utilisateurs (Admin uniquement)
        /// </summary>
        /// <returns>Liste de tous les utilisateurs</returns>
        /// <response code="200">Liste récupérée avec succès</response>
        /// <response code="401">Non authentifié</response>
        /// <response code="403">Permission refusée (non Admin)</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]  // ← Seuls les Admins
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers()
        {
            IEnumerable<UserDto> users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// Récupérer un utilisateur par son ID
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <returns>Informations de l'utilisateur</returns>
        /// <response code="200">Utilisateur trouvé</response>
        /// <response code="401">Non authentifié</response>
        /// <response code="404">Utilisateur introuvable</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(int userId)
        {
            UserDto? user = await _userService.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = $"Utilisateur avec l'ID {userId} introuvable." });
            }

            return Ok(user);
        }

        /// <summary>
        /// Récupérer un utilisateur par son email
        /// </summary>
        /// <param name="email">Email de l'utilisateur</param>
        /// <returns>Informations de l'utilisateur</returns>
        /// <response code="200">Utilisateur trouvé</response>
        /// <response code="401">Non authentifié</response>
        /// <response code="404">Utilisateur introuvable</response>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            UserDto? user = await _userService.GetByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new { message = $"Utilisateur avec l'email {email} introuvable." });
            }

            return Ok(user);
        }

        /// <summary>
        /// Récupérer les utilisateurs par rôle (Admin uniquement)
        /// </summary>
        /// <param name="role">Rôle recherché (Client, Organizer, Admin)</param>
        /// <returns>Liste des utilisateurs avec ce rôle</returns>
        /// <response code="200">Liste récupérée</response>
        /// <response code="401">Non authentifié</response>
        /// <response code="403">Permission refusée (non Admin)</response>
        [HttpGet("by-role/{role}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            // Valider que le rôle existe
            if (!Enum.TryParse<Domain.Enums.UserRole>(role, ignoreCase: true, out var userRole))
            {
                return BadRequest(new { message = $"Rôle invalide : {role}. Valeurs acceptées : Client, Organizer, Admin" });
            }

            IEnumerable<UserDto> users = await _userService.GetByRoleAsync(userRole);
            return Ok(users);
        }

        /// <summary>
        /// Modifier les informations d'un utilisateur
        /// Seul l'utilisateur lui-même ou un Admin peut modifier
        /// </summary>
        /// <param name="userId">ID de l'utilisateur à modifier</param>
        /// <param name="dto">Nouvelles informations</param>
        /// <returns>Utilisateur modifié</returns>
        /// <response code="200">Modification réussie</response>
        /// <response code="401">Non authentifié</response>
        /// <response code="403">Permission refusée</response>
        /// <response code="404">Utilisateur introuvable</response>
        [HttpPut("{userId}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserDto dto)
        {
            try
            {
                // Extraire l'ID et le rôle de l'utilisateur connecté
                int currentUserId = GetCurrentUserId();
                bool isAdmin = User.IsInRole("Admin");

                // ✅ Vérifier les permissions : seul l'user lui-même ou un admin
                if (currentUserId != userId && !isAdmin)
                {
                    return Forbid();  // 403 Forbidden
                }

                // Modifier l'utilisateur
                UserDto updatedUser = await _userService.UpdateUserAsync(userId, dto);

                return Ok(updatedUser);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Supprimer un utilisateur (Admin uniquement)
        /// </summary>
        /// <param name="userId">ID de l'utilisateur à supprimer</param>
        /// <returns>Confirmation de suppression</returns>
        /// <response code="204">Suppression réussie</response>
        /// <response code="401">Non authentifié</response>
        /// <response code="403">Permission refusée (non Admin)</response>
        /// <response code="404">Utilisateur introuvable</response>
        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            bool deleted = await _userService.DeleteUserAsync(userId);

            if (!deleted)
            {
                return NotFound(new { message = $"Utilisateur avec l'ID {userId} introuvable." });
            }

            return NoContent();  // 204 No Content (succès sans corps de réponse)
        }

        /// <summary>
        /// Changer le mot de passe d'un utilisateur
        /// Nécessite le mot de passe actuel pour sécurité
        /// Seul l'utilisateur lui-même peut changer son mot de passe
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <param name="dto">Ancien et nouveau mot de passe</param>
        /// <returns>Confirmation du changement</returns>
        /// <response code="200">Mot de passe modifié</response>
        /// <response code="400">Ancien mot de passe incorrect</response>
        /// <response code="401">Non authentifié</response>
        /// <response code="403">Permission refusée</response>
        /// <response code="404">Utilisateur introuvable</response>
        [HttpPut("{userId}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                // ✅ Seul l'utilisateur lui-même peut changer son mot de passe
                int currentUserId = GetCurrentUserId();

                if (currentUserId != userId)
                {
                    return Forbid();  // 403 - Même un Admin ne peut pas changer le mot de passe d'un autre
                }

                // Changer le mot de passe
                await _userService.ChangePasswordAsync(userId, dto);

                return Ok(new { message = "Mot de passe modifié avec succès." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Changer l'email d'un utilisateur
        /// Nécessite le mot de passe actuel pour sécurité (réauthentification)
        /// Seul l'utilisateur lui-même peut changer son email
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <param name="dto">Nouveau email et mot de passe actuel</param>
        /// <returns>Utilisateur avec nouvel email</returns>
        /// <response code="200">Email modifié</response>
        /// <response code="400">Email déjà utilisé ou mot de passe incorrect</response>
        /// <response code="401">Non authentifié</response>
        /// <response code="403">Permission refusée</response>
        /// <response code="404">Utilisateur introuvable</response>
        [HttpPut("{userId}/email")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeEmail(int userId, [FromBody] ChangeEmailDto dto)
        {
            try
            {
                // Seul l'utilisateur lui-même peut changer son email
                int currentUserId = GetCurrentUserId();

                if (currentUserId != userId)
                {
                    return Forbid();  // 403
                }

                // Changer l'email
                UserDto updatedUser = await _userService.ChangeEmailAsync(userId, dto);

                return Ok(updatedUser);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Extraire l'ID de l'utilisateur connecté depuis le token JWT
        /// </summary>
        /// <returns>ID de l'utilisateur</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Token JWT invalide ou utilisateur non identifié.");
            }

            return userId;
        }

        #endregion
    }
}
