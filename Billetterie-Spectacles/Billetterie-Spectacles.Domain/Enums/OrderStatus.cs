namespace Billetterie_Spectacles.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,            // En attente de paiement
        PaymentConfirmed = 1,   // Paiement confirmé
        PaymentFailed = 2,      // Paiement échoué
        Cancelled = 3,          // Commande Annulée
        Refunded = 4            // Commande Remboursée
    }
}
