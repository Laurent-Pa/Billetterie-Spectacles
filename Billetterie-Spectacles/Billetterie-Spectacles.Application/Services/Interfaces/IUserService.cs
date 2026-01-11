using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.DTO.UserSensitive;
using Billetterie_Spectacles.Domain.Enums;

namespace Billetterie_Spectacles.Application.Services.Interfaces
{
        /// <summary>
        /// Service de gestion des utilisateurs
        /// Orchestre la logique métier liée aux utilisateurs
        /// </summary>
        public interface IUserService
        {
            // Gestion des utilisateurs
            Task<UserDto?> GetByIdAsync(int userId);
            Task<UserDto?> GetByEmailAsync(string userEmail);
            Task<IEnumerable<UserDto>> GetAllAsync();
            Task<IEnumerable<UserDto>> GetByRoleAsync(UserRole userRole);

            // Création et modification
            Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
            Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
            Task<bool> DeleteUserAsync(int userId);

            // Opérations spécifiques
            Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
            Task<bool> ChangeEmailAsync(int userId, ChangeEmailDto changeEmailDto);
            Task<bool> EmailExistsAsync(string userEmail);

            // Statistiques
            Task<int> CountByRoleAsync(UserRole userRole);
        }
}
