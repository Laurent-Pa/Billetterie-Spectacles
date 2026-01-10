using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Request
{
    /// <summary>
    /// DTO pour la mise à jour du profil d'un utilisateur (méthode PUT)
    /// Contient uniquement les champs modifiables par l'utilisateur lui-même 
    /// </summary>
    public class UpdateUserDto
    {
        // l'email et le mot de passe ne sont pas modifiables avec ce DTO
        // car ils nécessitent de valider le mot de passe à cause de la sensibilité des informations

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom de famille est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le nom de famille ne peut pas dépasser 100 caractères")]
        public string Surname { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [MaxLength(20)]
        public string? Phone { get; set; }
    }
}
