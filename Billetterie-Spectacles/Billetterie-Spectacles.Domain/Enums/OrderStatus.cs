namespace Billetterie_Spectacles.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,      // En attente de paiement
        Paid = 1,         // Payée
        Cancelled = 2,    // Annulée
        Refunded = 3      // Remboursée
    }
}
