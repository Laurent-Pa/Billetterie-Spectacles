using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.Helpers;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Billetterie_Spectacles.Presentation.Controllers
{
    /// <summary>
    /// Contrôleur d'authentification - Gère l'inscription et la connexion
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IUserService userService, IAuthenticationService authenticationService, JwtTokenHelper jwtTokenHelper) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IAuthenticationService _authenticationService = authenticationService;
        private readonly JwtTokenHelper _jwtTokenHelper = jwtTokenHelper;


        /// <summary>
        /// Inscription d'un nouvel utilisateur
        /// </summary>
        /// <param name="dto">Informations d'inscription</param>
        /// <returns>Utilisateur créé avec token JWT</returns>
        /// <response code="201">Utilisateur créé avec succès</response>
        /// <response code="400">Données invalides ou email déjà utilisé</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            try
            {
                // Créer l'utilisateur
                UserDto user = await _userService.CreateUserAsync(dto);

                // Générer le token JWT
                string token = _jwtTokenHelper.GenerateToken(user);

                // Retourner la réponse
                AuthResponseDto response = new (
                    Token: token,
                    User: user
                );

                return CreatedAtAction(
                    actionName: nameof(GetCurrentUser),
                    routeValues: null,
                    value: response
                );
            }
            catch (DomainException ex)
            {
                // Email déjà utilisé ou autre erreur métier
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validation échouée
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Connexion d'un utilisateur existant
        /// </summary>
        /// <param name="dto">Identifiants de connexion</param>
        /// <returns>Token JWT si connexion réussie</returns>
        /// <response code="200">Connexion réussie</response>
        /// <response code="401">Email ou mot de passe incorrect</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Tenter la connexion
            UserDto? user = await _authenticationService.LoginAsync(dto.Email, dto.Password);

            if (user == null)
            {
                // Identifiants invalides
                return Unauthorized(new { message = "Email ou mot de passe incorrect." });
            }

            // Générer le token JWT
            string token = _jwtTokenHelper.GenerateToken(user);

            // Retourner la réponse
            AuthResponseDto response = new (
                Token: token,
                User: user
            );

            return Ok(response);
        }

        /// <summary>
        /// Obtenir les informations de l'utilisateur actuellement connecté
        /// Nécessite un token JWT valide dans le header Authorization
        /// </summary>
        /// <returns>Informations de l'utilisateur connecté</returns>
        /// <response code="200">Informations récupérées avec succès</response>
        /// <response code="401">Token manquant, invalide ou expiré</response>
        /// <response code="404">Utilisateur introuvable (token valide mais utilisateur supprimé)</response>
        [HttpGet("me")]
        [Authorize]  // ← Connaître l'utilisateur nécessite l'authentification (Token JWT valide)
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Extraire l'ID utilisateur du token JWT
            // User est automatiquement rempli par ASP.NET Core après validation du token
            Claim? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Token invalide ou malformé." });
            }

            // Récupérer l'utilisateur depuis la base de données
            UserDto? user = await _userService.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "Utilisateur introuvable." });
            }

            return Ok(user);
        }

    }
}
