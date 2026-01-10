using Billetterie_Spectacles.Application.DTO.Validation;
using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Request
{
    /// <summary>
    /// DTO pour la mise à jour d'un spectacle existant
    /// Seul le créateur du spectacle peut le modifier
    /// </summary>
    public class UpdateSpectacleDto
    {
        [Required(ErrorMessage = "Le nom du spectacle est obligatoire")]
        [MaxLength(200, ErrorMessage = "Le nom ne peut pas dépasser 200 caractères")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La catégorie est obligatoire")]
        [ValidSpectacleCategory] // On valide la saisie correspondant à l'enum
        public string Category { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "La description ne peut pas dépasser 1000 caractères")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "La durée est obligatoire")]
        [Range(1, 600, ErrorMessage = "La durée doit être entre 1 et 600 minutes (10 heures)")] // limite les valeurs aberrantes
        public int Duration { get; set; }

        [Url(ErrorMessage = "L'URL de l'image n'est pas valide")]   // exemple d'URL valide: "https://cdn.example.com/image.jpg"
        [MaxLength(500)]
        public string? Thumbnail { get; set; }
    }
}
