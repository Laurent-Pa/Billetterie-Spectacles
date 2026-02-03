using Billetterie_Spectacles.Application.DTO.Response;

namespace Billetterie_Spectacles.Application.Services.Interfaces
{
    public interface IPaymentHttpService
    {
        Task<PaymentResponseDto?> ProcessPaymentAsync(
            decimal amount,
            string currency,
            string orderId,
            string? paymentMethodId = null,
            string? customerEmail = null,
            string? description = null);
        Task<PaymentResponseDto?> GetPaymentStatusAsync(string paymentId);
    }
}