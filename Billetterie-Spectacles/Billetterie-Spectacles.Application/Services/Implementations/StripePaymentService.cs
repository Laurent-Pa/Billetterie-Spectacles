////using Stripe
//using Billetterie_Spectacles.Application.Services.Interfaces;
//using Microsoft.Extensions.Configuration;

//namespace Billetterie_Spectacles.Application.Services.Implementations
//{
//    internal class StripePaymentService : IPaymentService
//    {
//        private readonly string _apiKey;

//        public StripePaymentService(IConfiguration configuration)
//        {
//            _apiKey = configuration["Stripe:SecretKey"];
//            StripeConfiguration.ApiKey = _apiKey;
//        }

//        public async Task<PaymentResult> ProcessPaymentAsync(
//            decimal amount,
//            int userId,
//            int performanceId)
//        {
//            try
//            {
//                var options = new PaymentIntentCreateOptions
//                {
//                    Amount = (long)(amount * 100), // Stripe utilise des centimes
//                    Currency = "eur",
//                    Metadata = new Dictionary<string, string>
//                    {
//                        { "userId", userId.ToString() },
//                        { "performanceId", performanceId.ToString() }
//                    }
//                };

//                var service = new PaymentIntentService();
//                var paymentIntent = await service.CreateAsync(options);

//                return PaymentResult.Success(paymentIntent.Id);
//            }
//            catch (StripeException ex)
//            {
//                return PaymentResult.Failure(ex.Message);
//            }
//        }

//        public async Task<bool> RefundPaymentAsync(string paymentIntentId)
//        {
//            try
//            {
//                var service = new RefundService();
//                await service.CreateAsync(new RefundCreateOptions
//                {
//                    PaymentIntent = paymentIntentId
//                });
//                return true;
//            }
//            catch (StripeException)
//            {
//                return false;
//            }
//        }
//    }
//}
//}
