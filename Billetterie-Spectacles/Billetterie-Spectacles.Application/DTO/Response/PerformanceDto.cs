namespace Billetterie_Spectacles.Application.DTO.Response
{
    /// <summary>
    /// DTO pour la lecture des informations d'une performance (représentation)
    /// Utilisé pour afficher une date de spectacle avec sa disponibilité
    /// </summary>
    public record PerformanceDto(
        int Id,
        DateTime Date,
        string Status,  // conversion depuis l'enum --> Status: performance.Status.ToString()
        int Capacity,
        decimal UnitPrice,
        int AvailableTickets,
        int SpectacleId
    );
}
