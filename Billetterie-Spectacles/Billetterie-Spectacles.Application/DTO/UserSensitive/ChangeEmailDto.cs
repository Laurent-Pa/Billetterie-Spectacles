using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.UserSensitive
{
    /// <summary>
    /// DTO pour la demande de changement d'email
    /// Nécessite le mot de passe actuel pour des raisons de sécurité
    /// </summary>
    public class ChangeEmailDto
    {
        [Required(ErrorMessage = "La nouvelle adresse email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]    // vérification par REGEX interne ^[^@\s]+@[^@\s]+\.[^@\s]+$
        [MaxLength(255)]
        public string NewEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe actuel est obligatoire")]
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
