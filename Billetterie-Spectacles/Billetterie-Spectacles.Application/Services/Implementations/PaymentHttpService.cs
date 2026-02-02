using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Billetterie_Spectacles.Application.Services.Implementations
{
    public class PaymentHttpService : IPaymentHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentHttpService> _logger;

        public PaymentHttpService(HttpClient httpClient, ILogger<PaymentHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PaymentResponseDto?> ProcessPaymentAsync(decimal amount, string currency, string orderId)
        {
            try
            {
                var request = new PaymentRequestDto
                {
                    Amount = amount,
                    Currency = currency,
                    OrderId = orderId
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling Payment Microservice for order {OrderId}, amount {Amount} {Currency}",
                    orderId, amount, currency);

                var response = await _httpClient.PostAsync("/api/payments", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var paymentResponse = JsonSerializer.Deserialize<PaymentResponseDto>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Payment processed successfully: {PaymentId}", paymentResponse?.PaymentIntentId);
                    return paymentResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Payment microservice returned error {StatusCode}: {Error}",
                        response.StatusCode, errorContent);

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<PaymentResponseDto>(errorContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return errorResponse;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling payment microservice for order {OrderId}", orderId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling payment microservice for order {OrderId}", orderId);
                return null;
            }
        }

        public async Task<PaymentResponseDto?> GetPaymentStatusAsync(string PaymentIntentId)
        {
            try
            {
                _logger.LogInformation("Getting payment status for {PaymentId}", PaymentIntentId);

                var response = await _httpClient.GetAsync($"/api/payments/{PaymentIntentId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var paymentResponse = JsonSerializer.Deserialize<PaymentResponseDto>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return paymentResponse;
                }
                else
                {
                    _logger.LogWarning("Payment {PaymentId} not found or error: {StatusCode}",
                        PaymentIntentId, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for {PaymentId}", PaymentIntentId);
                return null;
            }
        }
    }
}