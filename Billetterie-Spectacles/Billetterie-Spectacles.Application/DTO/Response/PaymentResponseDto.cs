namespace Billetterie_Spectacles.Application.DTO.Response
{
    public class PaymentResponseDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TransactionId { get; set; }
    }
}