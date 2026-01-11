using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Application.Mappings;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Domain.Entities;

namespace Billetterie_Spectacles.Application.Services.Implementations
{
    /// <summary>
    /// Implémentation du service d'authentification
    /// </summary>
    public class AuthenticationService(IUserRepository _userRepository) : IAuthenticationService
    {
        /// <summary>
        /// Authentifie un utilisateur avec son email et mot de passe
        /// </summary>
        public async Task<UserDto?> LoginAsync(string email, string password)
        {
            // Récupérer l'utilisateur par email
            User? user = await _userRepository.GetByEmailAsync(email);

            // Si l'utilisateur n'existe pas, retourner null
            if (user == null)
            {
                return null;
            }

            // Vérifier le mot de passe avec BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

            // Si le mot de passe est invalide, retourner null
            if (!isPasswordValid)
            {
                return null;
            }

            // Authentification réussie : retourner le DTO
            return UserMapper.EntityToDto(user);
        }

        /// <summary>
        /// Valide les credentials d'un utilisateur sans retourner ses informations
        /// </summary>
        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            // Récupérer l'utilisateur par email
            User? user = await _userRepository.GetByEmailAsync(email);

            // Si l'utilisateur n'existe pas, credentials invalides
            if (user == null)
            {
                return false;
            }

            // Vérifier le mot de passe avec BCrypt
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        /// <summary>
        /// Vérifie si un utilisateur existe par son email
        /// </summary>
        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }
    }
}
