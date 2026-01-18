namespace Billetterie_Spectacles.Application.DTO.Response
{
    /// <summary>
    /// DTO de réponse pour l'authentification
    /// Contient le token JWT et les informations de l'utilisateur
    /// </summary>
    public record AuthResponseDto(
        string Token,
        UserDto User
    );
}
