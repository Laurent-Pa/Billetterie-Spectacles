namespace Billetterie_Spectacles.Application.Services.Interfaces
{
    /// <summary>
    /// Interface pour l'abstraction du service de paiement
    /// Permet de brancher différentes implémentations (Mock, Stripe, etc.)
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Traite un paiement pour une commande
        /// </summary>
        /// <param name="totalPrice">Montant à payer en euros</param>
        /// <param name="userId">ID de l'utilisateur qui paie</param>
        /// <param name="performanceId">ID de la performance</param>
        /// <returns>Résultat du paiement avec ID de transaction</returns>
        Task<PaymentResult> ProcessPaymentAsync(decimal totalPrice, int userId);

        /// <summary>
        /// Annule/rembourse un paiement (pour annulation de commande)
        /// </summary>
        /// <param name="paymentIntentId">ID de la transaction à annuler</param>
        /// <returns>True si le remboursement a réussi</returns>
        Task<bool> RefundPaymentAsync(string paymentIntentId);
    }

    /// <summary>
    /// Résultat d'une tentative de paiement
    /// </summary>
    public class PaymentResult
    {
        /// <summary>
        /// Indique si le paiement a réussi
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// ID de la transaction (Payment Intent pour Stripe)
        /// Permet de suivre/annuler le paiement plus tard
        /// </summary>
        public string? PaymentIntentId { get; set; }

        /// <summary>
        /// Message d'erreur si le paiement a échoué
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Constructeur pour succès
        /// </summary>
        public static PaymentResult Success(string paymentIntentId)
        {
            return new PaymentResult
            {
                IsSuccess = true,
                PaymentIntentId = paymentIntentId
            };
        }

        /// <summary>
        /// Constructeur pour échec
        /// </summary>
        public static PaymentResult Failure(string errorMessage)
        {
            return new PaymentResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
