using Billetterie_Spectacles.Application.DTO.Validation;
using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Request
{
    /// <summary>
    /// DTO pour la création d'une nouvelle performance (représentation)
    /// Utilisé par les organisateurs pour ajouter une date à leur spectacle
    /// </summary>
    public class CreatePerformanceDto
    {
        // Par défaut le statut d'une nouvelle représentation est "Scheduled"
        // Par défaut le nombre AvailableTickets est égal à Capacity à la création (logique métier dans l'entité Domain)

        [Required(ErrorMessage = "La date de la représentation est obligatoire")]
        [FutureDate(ErrorMessage = "La représentation doit être planifiée dans le futur")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "La capacité est obligatoire")]
        [Range(1, 100000, ErrorMessage = "La capacité doit être entre 1 et 100 000 places")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Le prix unitaire d'une représentation est obligatoire")]
        [Range(0, 10000, ErrorMessage = "Le prix unitaire doit être entre 0 et 10 000€")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "L'identifiant du spectacle est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "L'identifiant du spectacle doit être valide")]
        public int SpectacleId { get; set; }
    }
}
