using Billetterie_Spectacles.Application.DTO.Validation;
using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Request
{
    /// <summary>
    /// DTO pour la mise à jour d'une performance existante
    /// Permet de modifier la date, la capacité et le statut
    /// </summary>
    public class UpdatePerformanceDto
    {
        // AvailableTickets sera recalculé en fonction de la nouvelle capacité et des places déjà vendues

        [Required(ErrorMessage = "La date de la représentation est obligatoire")]
        [FutureDate(ErrorMessage = "La représentation doit être planifiée dans le futur")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "La capacité est obligatoire")]
        [Range(1, 100000, ErrorMessage = "La capacité doit être entre 1 et 100 000 places")]
        public int Capacity { get; set; }   // valider la nouvelle capacité par rapport au nbre de places vendues

        [Required(ErrorMessage = "Le prix est obligatoire")]
        [Range(0, 10000, ErrorMessage = "Le prix doit être entre 0 et 10 000€")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Le statut est obligatoire")]
        [ValidPerformanceStatus]    // Valeurs valides : "Scheduled", "SoldOut", "Cancelled", "Completed"
        public string Status { get; set; } = string.Empty;
    }
}
