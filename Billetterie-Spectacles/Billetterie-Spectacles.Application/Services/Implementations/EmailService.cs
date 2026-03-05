using Billetterie_Spectacles.Application.Services.Interfaces;
using System;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

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
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = BuildOrderConfirmationHtml(orderId, toName, totalPrice, tickets);

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

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string toName)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpFromName, _smtpFromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = "Bienvenue à L’Usine à Émotions";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = BuildWelcomeHtml(toName);

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpHost, _smtpPort, _smtpUseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email de bienvenue envoyé avec succès à {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'envoi de l'email de bienvenue à {toEmail}");
                return false;
            }
        }

        private string BuildOrderConfirmationHtml(
            int orderId,
            string customerName,
            decimal totalPrice,
            IEnumerable<OrderTicketInfo> tickets)
        {
            var fakeQrSvg = @"
<svg xmlns=""http://www.w3.org/2000/svg"" width=""160"" height=""160"" viewBox=""0 0 160 160"">
  <rect width=""160"" height=""160"" fill=""#ffffff""/>
  <rect x=""8"" y=""8"" width=""40"" height=""40"" fill=""#000000""/>
  <rect x=""14"" y=""14"" width=""28"" height=""28"" fill=""#ffffff""/>
  <rect x=""20"" y=""20"" width=""16"" height=""16"" fill=""#000000""/>
  <rect x=""112"" y=""8"" width=""40"" height=""40"" fill=""#000000""/>
  <rect x=""118"" y=""14"" width=""28"" height=""28"" fill=""#ffffff""/>
  <rect x=""124"" y=""20"" width=""16"" height=""16"" fill=""#000000""/>
  <rect x=""8"" y=""112"" width=""40"" height=""40"" fill=""#000000""/>
  <rect x=""14"" y=""118"" width=""28"" height=""28"" fill=""#ffffff""/>
  <rect x=""20"" y=""124"" width=""16"" height=""16"" fill=""#000000""/>

  <rect x=""56"" y=""8"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""72"" y=""8"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""88"" y=""8"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""56"" y=""24"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""72"" y=""24"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""96"" y=""24"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""56"" y=""40"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""80"" y=""40"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""96"" y=""40"" width=""8"" height=""8"" fill=""#000000""/>

  <rect x=""56"" y=""56"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""64"" y=""64"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""80"" y=""64"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""96"" y=""64"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""104"" y=""72"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""56"" y=""72"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""72"" y=""72"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""88"" y=""72"" width=""8"" height=""8"" fill=""#000000""/>

  <rect x=""56"" y=""88"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""72"" y=""88"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""88"" y=""88"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""104"" y=""88"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""64"" y=""96"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""80"" y=""96"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""96"" y=""96"" width=""8"" height=""8"" fill=""#000000""/>

  <rect x=""56"" y=""112"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""72"" y=""112"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""88"" y=""112"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""104"" y=""112"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""64"" y=""120"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""80"" y=""120"" width=""8"" height=""8"" fill=""#000000""/>
  <rect x=""96"" y=""120"" width=""8"" height=""8"" fill=""#000000""/>
</svg>";
            var fakeQrDataUri = "data:image/svg+xml;base64," +
                Convert.ToBase64String(Encoding.UTF8.GetBytes(fakeQrSvg));

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
        .header {{ background-color: #0b0b0b; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
        .order-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .ticket-block {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; text-align: center; }}
        .qr {{ display: inline-block; padding: 10px; background: #ffffff; border: 1px solid #e0e0e0; border-radius: 6px; }}
        table {{ width: 100%; border-collapse: collapse; background-color: white; }}
        th {{ background-color: #b3161c; color: white; padding: 10px; text-align: left; }}
        .total {{ font-weight: bold; font-size: 1.2em; text-align: right; padding: 15px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 0.9em; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1 style=""margin: 0; font-size: 20px; letter-spacing: 0.08em; text-transform: uppercase;"">L’Usine à Émotions</h1>
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

            <div class=""ticket-block"">
                <h3>Voici votre billet</h3>
                <p>Présentez ce QR code à l’entrée.</p>
                <div class=""qr"">
                    <img src=""{fakeQrDataUri}"" alt=""QR code"" width=""160"" height=""160"" />
                </div>
            </div>

            <p>Merci pour votre confiance !</p>
        </div>
        <div class=""footer"">
            <p>L’Usine à Émotions - Service Client</p>
            <p>Cet email a été envoyé automatiquement, merci de ne pas y répondre.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string BuildWelcomeHtml(string customerName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0b0b0b; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
        .cta {{ display: inline-block; padding: 10px 16px; background-color: #b3161c; color: white; text-decoration: none; border-radius: 4px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 0.9em; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1 style=""margin: 0; font-size: 20px; letter-spacing: 0.08em; text-transform: uppercase;"">L’Usine à Émotions</h1>
        </div>
        <div class=""content"">
            <h2>Bienvenue {customerName} !</h2>
            <p>Votre compte est bien créé. Vous pouvez dès maintenant découvrir la programmation et réserver vos places.</p>
            <p>
                <a class=""cta"" href=""http://localhost:3000/programmation"">Voir la programmation</a>
            </p>
            <p>Merci pour votre confiance et à très vite !</p>
        </div>
        <div class=""footer"">
            <p>L’Usine à Émotions - Service Client</p>
            <p>Cet email a été envoyé automatiquement, merci de ne pas y répondre.</p>
        </div>
    </div>
</body>
</html>";
        }

    }
}
