using Billetterie_Spectacles.Application.DTO.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Billetterie_Spectacles.Application.Services.Interfaces
{  
    /// <summary>
    /// Service d'authentification
    /// Gère le login et la validation des credentials
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authentifie un utilisateur avec son email et mot de passe
        /// </summary>
        /// <param name="email">Email de l'utilisateur</param>
        /// <param name="password">Mot de passe en clair</param>
        /// <returns>UserDto si authentification réussie, null sinon</returns>
        Task<UserDto?> LoginAsync(string email, string password);

        /// <summary>
        /// Valide les credentials d'un utilisateur
        /// </summary>
        /// <param name="email">Email de l'utilisateur</param>
        /// <param name="password">Mot de passe en clair</param>
        /// <returns>True si les credentials sont valides, false sinon</returns>
        Task<bool> ValidateCredentialsAsync(string email, string password);

        /// <summary>
        /// Vérifie si un utilisateur existe par son email
        /// </summary>
        /// <param name="email">Email à vérifier</param>
        /// <returns>True si l'utilisateur existe, false sinon</returns>
        Task<bool> UserExistsAsync(string email);
    }
}
