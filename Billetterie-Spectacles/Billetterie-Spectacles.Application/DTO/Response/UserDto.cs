namespace Billetterie_Spectacles.Application.DTO.Response
{
    /// <summary>
    /// DTO pour la lecture des informations d'un utilisateur
    /// Utilisé pour afficher un profil utilisateur ou dans une liste
    /// </summary>
    public record UserDto(
        int Id,
        string Name,
        string Surname,
        string Email,
        string? Phone,
        string Role,        // conversion de l'enum dans le mapping --> Role: user.Role.ToString()
        DateTime CreatedAt
    );
}
