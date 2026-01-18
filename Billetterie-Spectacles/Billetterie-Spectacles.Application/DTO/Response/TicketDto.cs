namespace Billetterie_Spectacles.Application.DTO.Response
{
    /// <summary>
    /// DTO pour la lecture des informations d'un billet
    /// Un billet est associé à une commande et une performance
    /// Pas de CreateTicketDto: les billets sont créés automatiquement lors d'une commande, pas de puis l'API
    /// </summary>
    public record TicketDto(
        int Id,
        string Status,
        decimal UnitPrice,
        int OrderId,            // permettra d'afficher les billets d'une commande (pour client)
        int PerformanceId,       // permettra d'afficher les billets d'une représentation (pour organisateur)
        DateTime CreatedAt,     // suis la date de création de la commande correspondante
        DateTime UpdatedAt,      // suis la commande ou la vie individuelle du ticket
        PerformanceDto? Performance = null  // relation ticket --> performance
    );
}
