using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Security.Claims;

namespace Billetterie_Spectacles.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Toutes les commandes nécessitent d'être authentifié
    public class OrdersController(
        IOrderService orderService,
        IPerformanceService performanceService,
        IHttpClientFactory httpClientFactory,
        ILogger<OrdersController> logger
        ) : ControllerBase
    {
        private readonly IOrderService _orderService = orderService;
        private readonly IPerformanceService _performanceService = performanceService;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("PaymentService");
        private readonly ILogger<OrdersController> _logger = logger;

        // Méthodes helper

        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("ID utilisateur introuvable dans le token JWT");
            }

            return int.Parse(userIdClaim);
        }

        private UserRole GetCurrentUserRole()
        {
            string? roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(roleClaim))
            {
                throw new UnauthorizedAccessException("Rôle utilisateur introuvable dans le token JWT");
            }

            return Enum.Parse<UserRole>(roleClaim);
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut consulter une commande
        /// (propriétaire ou admin)
        /// </summary>
        private async Task<bool> CanViewOrder(int orderId)
        {
            var currentUserRole = GetCurrentUserRole();

            // Les admins peuvent tout voir
            if (currentUserRole == UserRole.Admin)
                return true;

            // Les autres utilisateurs ne peuvent voir que leurs propres commandes
            OrderDto? order = await _orderService.GetByIdAsync(orderId);
            if (order == null)
                return false;

            int currentUserId = GetCurrentUserId();
            return order.UserId == currentUserId;
        }

        /// <summary>
        /// Crée une nouvelle commande avec paiement
        /// </summary>
        /// <param name="dto">Détails de la commande (performances + quantités)</param>
        /// <returns>La commande créée avec statut de paiement</returns>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                // Récupérer l'ID de l'utilisateur connecté
                int currentUserId = GetCurrentUserId();

                // Créer la commande (inclut le traitement du paiement)
                OrderDto createdOrder = await _orderService.CreateOrderAsync(dto, currentUserId);

                // Retourner 201 Created
                return Created(
                    $"/api/orders/{createdOrder.Id}",
                    createdOrder
                );
            }
            catch (NotFoundException ex)
            {
                // Performance ou User introuvable
                return NotFound(new { message = ex.Message });
            }
            catch (DomainException ex)
            {
                // Validation métier ou paiement échoué
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Une erreur est survenue lors de la création de la commande", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer les commandes de l'utilisateur connecté
        /// Accessible par : Client, Organizer
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                IEnumerable<OrderDto> orders = await _orderService.GetByUserIdAsync(currentUserId);

                if (orders == null || !orders.Any())
                {
                    return NotFound(new { message = $"Aucune commande trouvée pour l'id {currentUserId}" });
                }

                return Ok(orders);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des commandes", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer toutes les commandes avec filtres optionnels
        /// Accessible par : Admin, Organizer
        /// </summary>
        /// <param name="userId">Filtre optionnel par utilisateur</param>
        /// <param name="performanceId">Filtre optionnel par représentation</param>
        /// <param name="orderStatus">Filtre optionnel par statut</param>
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Organizer")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders(
            [FromQuery] int? userId = null,
            [FromQuery] int? performanceId = null,
            [FromQuery] OrderStatus? orderStatus = null)
        {
            try
            {
                IEnumerable<OrderDto> orders = await _orderService.GetAllWithFiltersAsync(
                    userId: userId,
                    performanceId: performanceId,
                    orderStatus: orderStatus
                );

                if (!orders.Any())
                {
                    return Ok(new List<OrderDto>());  // Retourne une liste vide plutôt que 404
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des commandes", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer les détails d'une commande spécifique avec ses tickets
        /// Accessible par : Client (ses commandes), Admin, Organizer (toutes)
        /// </summary>
        /// <param name="id">ID de la commande</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                UserRole currentUserRole = GetCurrentUserRole();

                // Récupérer la commande avec ses tickets
                OrderDto? order = await _orderService.GetWithTicketsAsync(id);

                if (order == null)
                {
                    return NotFound(new { message = $"Commande avec l'ID {id} introuvable." });
                }

                // Vérification des permissions
                // Client : peut voir uniquement ses propres commandes
                // Admin et Organizer : peuvent voir toutes les commandes
                if (currentUserRole == UserRole.Client && order.UserId != currentUserId)
                {
                    return Forbid();  // 403 Forbidden
                }

                return Ok(order);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération de la commande", error = ex.Message });
            }
        }

        /// <summary>
        /// Vérifie auprès du microservice de paiement que le PaymentIntent est bien succeeded
        /// </summary>
        private async Task<bool> VerifyPaymentWithStripe(string paymentIntentId)
        {
            try
            {
                // Appelle ton microservice de paiement
                var response = await _httpClient.GetAsync($"api/payments/{paymentIntentId}");

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();

                // Vérifier que le statut est "succeeded"
                return paymentResponse?.Status?.ToLower() == "succeeded";
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Confirme une commande après un paiement réussi
        /// </summary>
        [HttpPost("{id}/confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmOrder(int id, [FromBody] PaymentResponseDto dto)
        {
            try
            {
                // Récupérer la commande avec ses tickets
                OrderDto? order = await _orderService.GetWithTicketsAsync(id);

                if (order == null)
                {
                    return NotFound(new { message = $"Order {id} not found" });
                }

                // Vérifier que la commande est en attente de paiement
                if (order.Status != OrderStatus.Pending.ToString())
                {
                    return BadRequest(new { message = $"Order {id} is not pending. Current status: {order.Status}" });
                }

                // Vérifier le paiement auprès du microservice Stripe
                bool paymentVerified = await VerifyPaymentWithStripe(dto.PaymentIntentId);

                if (!paymentVerified)
                {
                    return BadRequest(new { message = "Payment verification failed" });
                }

                // Confirmer la commande via le service
                await _orderService.ConfirmOrderAsync(id, dto.PaymentIntentId);

                return Ok(new
                {
                    message = "Order confirmed successfully",
                    orderId = id
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
