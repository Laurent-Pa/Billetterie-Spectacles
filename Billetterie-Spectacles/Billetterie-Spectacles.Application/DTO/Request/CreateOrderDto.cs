using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Request
{
    /// <summary>
    /// DTO pour la création d'une nouvelle commande
    /// L'utilisateur spécifie les représentations et le nombre de billets pour chacune
    /// </summary>
    public class CreateOrderDto
    {
        // L'ID de l'utilisateur n'est pas géré ici (token JWT)
        // Pas de prix à ce niveau, géré par la base en fonction du prix de la représentation (calculé côté service)

        [Required(ErrorMessage = "Au moins un billet doit être commandé")]
        [MinLength(1, ErrorMessage = "Au moins un billet doit être commandé")]
        public List<OrderItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Représente un élément de commande : une représentation et le nombre de billets
    /// Un utilisateur peut commander plusieurs billets pour la même représentation
    /// </summary>
    public class OrderItemDto
    {
        [Required(ErrorMessage = "L'identifiant de la performance est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "L'identifiant de la représentation doit être valide")]
        public int PerformanceId { get; set; }

        [Required(ErrorMessage = "Le nombre de billets est obligatoire")]
        [Range(1, 50, ErrorMessage = "Vous pouvez commander entre 1 et 50 billets par représentation")]
        public int Quantity { get; set; }
    }
}
