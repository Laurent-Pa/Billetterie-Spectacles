namespace Billetterie_Spectacles.Application.DTO.Response
{
    /// <summary>
    /// DTO pour la lecture des informations d'une commande
    /// DTO minimaliste sans le détails de billets (usage Organisateur)
    /// </summary>
    public record OrderDto(
        int Id,
        string Status,  // conversion de l'enum dans le mapping --> Status: order.Status.ToString()
        decimal TotalPrice, // stocké dans la BDD, immuable même si le prix d'un billet du même spectacle évolue
        int UserId,
        DateTime CreatedAt,
        DateTime UpdatedAt, // pour suivi de màj de la commande (pending->paid ou autre)
        IEnumerable<TicketDto>? Tickets = null  // relation order--> ticket 
    );
}
