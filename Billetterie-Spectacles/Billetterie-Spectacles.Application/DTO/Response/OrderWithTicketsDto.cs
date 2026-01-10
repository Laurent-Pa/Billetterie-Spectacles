namespace Billetterie_Spectacles.Application.DTO.Response
{
    /// <summary>
    /// DTO pour la lecture des informations d'une commande
    /// DTO contenant le détail des billets (usage client)
    /// </summary>
    public record OrderWithTicketsDto(
        int Id,
        string Status,
        DateTime Date,
        decimal TotalPrice,
        int UserId,
        IEnumerable<TicketDto> Tickets  // ← Liste des billets inclus
    );
}
