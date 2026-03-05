//using Billetterie_Spectacles.Application.Services.Interfaces;

//namespace Billetterie_Spectacles.Application.Services.Implementations
//{
//    /// <summary>
//    /// Implémentation mock du service de paiement
//    /// Simule toujours un paiement réussi pour le développement
//    /// SERA REMPLACÉ par StripePaymentService plus tard
//    /// </summary>
//    public class MockPaymentService : IPaymentService
//    {
//        public Task<PaymentResult> ProcessPaymentAsync(decimal amount, int userId)
//        {
//            // Générer un faux Payment Intent ID
//            string? fakePaymentIntentId = $"mock_pi_{Guid.NewGuid().ToString().Substring(0, 8)}";

//            // Toujours retourner un succès pour l'instant
//            return Task.FromResult(PaymentResult.Success(fakePaymentIntentId));

//        }

//        public Task<bool> RefundPaymentAsync(string paymentIntentId)
//        {
//            // Simuler un remboursement réussi
//            return Task.FromResult(true);
//        }
//    }
//}
