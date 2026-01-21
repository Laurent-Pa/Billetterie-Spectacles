using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Request
{
    public class PaymentRequestDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code")]
        public string Currency { get; set; } = "EUR";

        [Required]
        public string OrderId { get; set; } = string.Empty;

        public string? CustomerEmail { get; set; }
        public string? Description { get; set; }
    }
}