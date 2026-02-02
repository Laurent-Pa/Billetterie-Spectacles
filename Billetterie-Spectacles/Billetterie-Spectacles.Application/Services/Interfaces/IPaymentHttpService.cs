using Billetterie_Spectacles.Application.DTO.Response;

namespace Billetterie_Spectacles.Application.Services.Interfaces
{
    public interface IPaymentHttpService
    {
        Task<PaymentResponseDto?> ProcessPaymentAsync(decimal amount, string currency, string orderId);
        Task<PaymentResponseDto?> GetPaymentStatusAsync(string paymentId);
    }
}