using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Request
{
    /// <summary>
    /// DTO pour la création d'un nouvel utilisateur (inscription)
    /// Utilisation des Data Annotation pour la validation (attributs traités automatiquement par ASP.NET)
    /// Propriétés avec set pour model binding, JSON deserialisé par ASP.NET
    /// </summary>
    public class CreateUserDto
    {
        // Id généré automatiquement par la base de donnée
        // CreatedAt généré automatiquement côté serveur
        // Role par défaut : "Client" (seul un admin peut créer un Organisateur)

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom de famille est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le nom de famille ne peut pas dépasser 100 caractères")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")] // vérification par REGEX interne ^[^@\s]+@[^@\s]+\.[^@\s]+$
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule, un chiffre et un caractère spécial")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [MaxLength(20)]
        public string? Phone { get; set; }
    }
}
