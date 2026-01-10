namespace Billetterie_Spectacles.Application.DTO.Response
{
    /// <summary>
    /// DTO pour la lecture des informations d'une commande
    /// DTO minimaliste sans le détails de billets (usage Organisateur)
    /// </summary>
    public record OrderDto(
        int Id,
        string Status,  // conversion de l'enum dans le mapping --> Status: order.Status.ToString()
        DateTime Date,  // pour rappel, c'est la date de création de la commande, pas du spectacle
        decimal TotalPrice, // stocké dans la BDD, immuable même si le prix d'un billet du même spectacle évolue
        int UserId
    );
}
