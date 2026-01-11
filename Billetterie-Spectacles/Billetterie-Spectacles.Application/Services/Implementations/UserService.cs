using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.DTO.UserSensitive;
using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Application.Mappings;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Billetterie_Spectacles.Application.Services.Implementations
{
    /// <summary>
    /// Implémentation du service de gestion des utilisateurs
    /// </summary>
    public class UserService(IUserRepository _userRepository) : IUserService
    {

        #region Consultation

        public async Task<UserDto?> GetByIdAsync(int userId)
        {
            User? user = await _userRepository.GetByIdAsync(userId);
            return user != null ? UserMapper.EntityToDto(user) : null;
        }

        public async Task<UserDto?> GetByEmailAsync(string userEmail)
        {
            User? user = await _userRepository.GetByEmailAsync(userEmail);
            return user != null ? UserMapper.EntityToDto(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            IEnumerable<User?> users = await _userRepository.GetAllAsync();
            return users.Select(UserMapper.EntityToDto);
        }

        public async Task<IEnumerable<UserDto>> GetByRoleAsync(UserRole userRole)
        {
            IEnumerable<User?> users = await _userRepository.GetByRoleAsync(userRole);
            return users.Select(UserMapper.EntityToDto);
        }

        public async Task<bool> EmailExistsAsync(string userEmail)
        {
            return await _userRepository.EmailExistsAsync(userEmail);
        }

        public async Task<int> CountByRoleAsync(UserRole userRole)
        {
            return await _userRepository.CountByRoleAsync(userRole);
        }

        #endregion

        #region Création et modification

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            // Validation : Email doit être unique
            if (await _userRepository.EmailExistsAsync(dto.Email))
            {
                throw new DomainException($"L'email {dto.Email} est déjà utilisé.");
            }

            // Hasher le mot de passe (logique métier)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Créer l'entité User (rôle Client par défaut)
            User user = new(
                name: dto.Name,
                surname: dto.Surname,
                email: dto.Email,
                password: hashedPassword,
                phone: dto.Phone,
                role: UserRole.Client  // Par défaut
            );

            // Sauvegarder en base
            User? createdUser = await _userRepository.AddAsync(user);

            // Expression conditionnelle, évite le if(user == null)
            return createdUser == null
                ? throw new DomainException("Erreur lors de la création de l'utilisateur.")
                : UserMapper.EntityToDto(createdUser);
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            // Vérifier que l'utilisateur existe
            User? user = await _userRepository.GetByIdAsync(id)  // expression de fusion pour simplifier if(user==null)
                ?? throw new NotFoundException($"Utilisateur avec l'ID {id} introuvable.");

            // Mettre à jour les propriétés modifiables
            user.UpdateProfile(dto.Name, dto.Surname, dto.Phone);

            // Sauvegarder les modifications
            User? updatedUser = await _userRepository.UpdateAsync(user);

            return UserMapper.EntityToDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        #endregion


        #region Opérations spécifiques

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            // Récupérer l'utilisateur
            User? user = await _userRepository.GetByIdAsync(userId) 
                ?? throw new NotFoundException($"Utilisateur avec l'ID {userId} introuvable.");

            // Vérifier que le mot de passe actuel est correct
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
            {
                throw new DomainException("Le mot de passe actuel est incorrect.");
            }

            // Vérifier que le nouveau mot de passe est différent
            if (dto.NewPassword == dto.CurrentPassword)
            {
                throw new DomainException("Le nouveau mot de passe doit être différent de l'ancien.");
            }

            // Hasher le nouveau mot de passe
            string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Mettre à jour le mot de passe
            user.ChangePassword(newHashedPassword);

            // Sauvegarder
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> ChangeEmailAsync(int userId, ChangeEmailDto dto)
        {
            // Récupérer l'utilisateur
            User? user = await _userRepository.GetByIdAsync(userId) 
                ?? throw new NotFoundException($"Utilisateur avec l'ID {userId} introuvable.");

            // Vérifier que le mot de passe actuel est correct (réauthentification)
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
            {
                throw new DomainException("Le mot de passe est incorrect.");
            }

            // Vérifier que le nouvel email n'est pas déjà utilisé
            if (await _userRepository.EmailExistsAsync(dto.NewEmail))
            {
                throw new DomainException($"L'email {dto.NewEmail} est déjà utilisé.");
            }

            // Vérifier que le nouvel email est différent
            if (dto.NewEmail.ToLower() == user.Email.ToLower())
            {
                throw new DomainException("Le nouvel email doit être différent de l'ancien.");
            }

            // Mettre à jour l'email
            user.ChangeEmail(dto.NewEmail);

            // Sauvegarder
            await _userRepository.UpdateAsync(user);

            return true;
        }

        #endregion

    }
}
