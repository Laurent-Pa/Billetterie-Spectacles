using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Domain.Entities;

namespace Billetterie_Spectacles.Application.Mappings
{
    // <summary>
    /// Mapper pour les conversions entre entité User et ses DTOs
    /// </summary>
    public static class UserMapper
    {
        /// <summary>
        /// Convertit une entité User en UserDto
        /// </summary>
        public static UserDto EntityToDto(User user)
        {
            return new UserDto(
                Id: user.UserId,
                Name: user.Name,
                Surname: user.Surname,
                Email: user.Email,
                Phone: user.Phone,
                Role: user.Role.ToString(),
                CreatedAt: user.CreatedAt
            );
        }

        /// <summary>
        /// Convertit un CreateUserDto en entité User
        /// Le mot de passe doit déjà être hashé
        /// </summary>
        public static User CreateDtoToEntity(CreateUserDto dto, string hashedPassword)
        {
            return new User(
                name: dto.Name,
                surname: dto.Surname,
                email: dto.Email,
                password: hashedPassword,
                phone: dto.Phone,
                role: Domain.Enums.UserRole.Client  // Par défaut
            );
        }

        /// <summary>
        /// Met à jour une entité User existante avec les données d'un UpdateUserDto
        /// </summary>
        public static void UpdateEntity(User user, UpdateUserDto dto)
        {
            user.UpdateProfile(dto.Name, dto.Surname, dto.Phone);
        }
    }
}
