using Billetterie_Spectacles.Application.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Billetterie_Spectacles.Application.Services.Implementations
{
    /// <summary>
    /// Implémentation du service d'envoi d'emails avec MailKit
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpFromEmail;
        private readonly string _smtpFromName;
        private readonly bool _smtpUseSsl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Configuration SMTP depuis appsettings.json
            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "localhost";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "1025");
            _smtpFromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@billetterie.com";
            _smtpFromName = _configuration["EmailSettings:FromName"] ?? "Billetterie Spectacles";
            _smtpUseSsl = bool.Parse(_configuration["EmailSettings:UseSsl"] ?? "false");
        }

        public async Task<bool> SendOrderConfirmationEmailAsync(
            string toEmail,
            string toName,
            int orderId,
            decimal totalPrice,
            IEnumerable<OrderTicketInfo> tickets)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpFromName, _smtpFromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = $"Confirmation de commande #{orderId} - Billetterie Spectacles";

                // Construction du corps de l'email en HTML
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = BuildOrderConfirmationHtml(orderId, toName, totalPrice, tickets)
                };

                message.Body = bodyBuilder.ToMessageBody();

                // Envoi via SMTP
                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpHost, _smtpPort, _smtpUseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                // MailHog n'a pas besoin d'authentification
                // Si vous utilisez un vrai serveur SMTP, ajoutez ici :
                // await client.AuthenticateAsync(username, password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email de confirmation envoyé avec succès à {toEmail} pour la commande #{orderId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'envoi de l'email de confirmation à {toEmail} pour la commande #{orderId}");
                return false;
            }
        }

        private string BuildOrderConfirmationHtml(
            int orderId,
            string customerName,
            decimal totalPrice,
            IEnumerable<OrderTicketInfo> tickets)
        {
            var ticketsList = string.Join("", tickets.Select(ticket => $@"
                <tr>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd;"">{ticket.SpectacleName}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd;"">{ticket.PerformanceDate:dd/MM/yyyy à HH:mm}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: right;"">{ticket.UnitPrice:F2} €</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: center;"">#{ticket.TicketId}</td>
                </tr>"));

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4a90e2; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
        .order-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        table {{ width: 100%; border-collapse: collapse; background-color: white; }}
        th {{ background-color: #4a90e2; color: white; padding: 10px; text-align: left; }}
        .total {{ font-weight: bold; font-size: 1.2em; text-align: right; padding: 15px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 0.9em; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎭 Billetterie Spectacles</h1>
        </div>
        <div class=""content"">
            <h2>Confirmation de votre commande</h2>
            <p>Bonjour {customerName},</p>
            <p>Nous vous confirmons la réception de votre commande <strong>#{orderId}</strong>.</p>
            
            <div class=""order-info"">
                <h3>Détails de votre commande</h3>
                <table>
                    <thead>
                        <tr>
                            <th>Spectacle</th>
                            <th>Date</th>
                            <th>Prix</th>
                            <th>Ticket #</th>
                        </tr>
                    </thead>
                    <tbody>
                        {ticketsList}
                    </tbody>
                </table>
                <div class=""total"">
                    Total : {totalPrice:F2} €
                </div>
            </div>
            
            <p>Vos billets sont confirmés. Vous recevrez un email séparé avec vos billets électroniques.</p>
            <p>Merci pour votre confiance !</p>
        </div>
        <div class=""footer"">
            <p>Billetterie Spectacles - Service Client</p>
            <p>Cet email a été envoyé automatiquement, merci de ne pas y répondre.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
