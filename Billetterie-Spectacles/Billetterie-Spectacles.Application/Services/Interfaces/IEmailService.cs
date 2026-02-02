namespace Billetterie_Spectacles.Application.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service d'envoi d'emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envoie un email de confirmation d'achat de billet
        /// </summary>
        /// <param name="toEmail">Email du destinataire</param>
        /// <param name="toName">Nom du destinataire</param>
        /// <param name="orderId">ID de la commande</param>
        /// <param name="totalPrice">Prix total de la commande</param>
        /// <param name="tickets">Liste des tickets achetés</param>
        /// <returns>True si l'email a été envoyé avec succès, false sinon</returns>
        Task<bool> SendOrderConfirmationEmailAsync(
            string toEmail,
            string toName,
            int orderId,
            decimal totalPrice,
            IEnumerable<OrderTicketInfo> tickets);

        /// <summary>
        /// Envoie un email de bienvenue après inscription
        /// </summary>
        /// <param name="toEmail">Email du destinataire</param>
        /// <param name="toName">Nom du destinataire</param>
        /// <returns>True si l'email a été envoyé avec succès, false sinon</returns>
        Task<bool> SendWelcomeEmailAsync(string toEmail, string toName);
    }

    /// <summary>
    /// Informations sur un ticket pour l'email de confirmation
    /// </summary>
    public record OrderTicketInfo(
        string SpectacleName,
        DateTime PerformanceDate,
        decimal UnitPrice,
        int TicketId
    );
}
