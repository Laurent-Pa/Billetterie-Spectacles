using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.UserSensitive
{
    /// <summary>
    /// DTO pour le changement de mot de passe d'un utilisateur connecté
    /// Nécessite l'ancien mot de passe pour des raisons de sécurité
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "L'ancien mot de passe est obligatoire")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire")]
        [MinLength(8, ErrorMessage = "Le nouveau mot de passe doit contenir au moins 8 caractères")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule, un chiffre et un caractère spécial")]
        [MaxLength(255)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule, un chiffre et un caractère spécial")]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
