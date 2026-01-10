namespace Billetterie_Spectacles.Application.DTO.Response
{
    /// <summary>
    /// DTO pour la lecture des informations d'un spectacle
    /// Utilisé pour afficher un spectacle ou dans une liste
    /// </summary>
    public record SpectacleDto(
            int Id,
            string Name,
            string Category,        // conversion de l'enum dans le mapping --> Category: spectacle.Category.ToString()
            string? Description,
            int Duration,           // entier en minutes (cf entité dans Domain)
            string? Thumbnail,
            DateTime CreatedAt,
            DateTime UpdatedAt
        );
}
