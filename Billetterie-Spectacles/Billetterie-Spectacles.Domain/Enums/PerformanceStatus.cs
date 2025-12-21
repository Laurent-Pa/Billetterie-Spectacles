namespace Billetterie_Spectacles.Domain.Enums
{
    public enum PerformanceStatus
    {
        Scheduled = 0,    // Programmée
        Cancelled = 1,    // Annulée
        Completed = 2,    // Terminée
        SoldOut = 3       // Complète (tous les billets vendus)
    }
}
