namespace Billetterie_Spectacles.Domain.Enums
{
    public enum TicketStatus
    {
        Reserved = 0,     // Réservé (pas encore payé)
        Paid = 1,         // Payé
        Used = 2,         // Utilisé (client a assisté au spectacle)
        Cancelled = 3     // Annulé
    }
}
