using Billetterie_Spectacles.Application.DTO.Request;
using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Application.Mappings;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Exceptions;

namespace Billetterie_Spectacles.Application.Services.Implementations
{
    /// <summary>
    /// Implémentation du service de gestion des commandes
    /// </summary>
    public class OrderService(
        IOrderRepository _orderRepository,                  // CRUD Order
        IPerformanceRepository _performanceRepository,      // Vérifier la dispo et réserver des places
        ITicketRepository _ticketRepository,                // Créer et gérer les tickets
        IUserRepository _userRepository,                     // Vérifier que l'user existe
        //IPaymentService _paymentService,                    // Service de paiment (interne appli)
        IPaymentHttpService _paymentHttpService             // Micro-Service de paiment (externe appli)
        ): IOrderService
    {
        #region Consultation

        public async Task<OrderDto?> GetByIdAsync(int orderId)
        {
            Order? order = await _orderRepository.GetByIdAsync(orderId);
            return order != null ? OrderMapper.EntityToDto(order) : null;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            IEnumerable<Order> orders = await _orderRepository.GetAllAsync();
            return orders.Select(OrderMapper.EntityToDto);
        }

        public async Task<IEnumerable<OrderDto>> GetAllWithFiltersAsync(
            int? userId = null,
            int? performanceId = null,
            OrderStatus? orderStatus = null)
        {
            IEnumerable<Order> orders = await _orderRepository.GetAllWithFiltersAsync(
                userId: userId,
                performanceId: performanceId,
                orderStatus: orderStatus
            );

            return orders.Select(OrderMapper.EntityToDto);
        }

        public async Task<IEnumerable<OrderDto>> GetByUserIdAsync(int userId)
        {
            IEnumerable<Order> orders = await _orderRepository.GetByUserIdAsync(userId);
            return orders.Select(OrderMapper.EntityToDto);
        }

        public async Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatus status)
        {
            IEnumerable<Order> orders = await _orderRepository.GetByStatusAsync(status);
            return orders.Select(OrderMapper.EntityToDto);
        }

        public async Task<OrderDto?> GetWithTicketsAsync(int orderId)
        {
            Order? order = await _orderRepository.GetWithTicketsAsync(orderId);
            return order != null ? OrderMapper.EntityToDto(order) : null;
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _orderRepository.GetTotalRevenueAsync();
        }

        public async Task<decimal> GetTotalRevenueByUserAsync(int userId)
        {
            return await _orderRepository.GetTotalRevenueByUserAsync(userId);
        }

        public async Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count = 10)
        {
            IEnumerable<Order> orders = await _orderRepository.GetRecentOrdersAsync(count);
            return orders.Select(OrderMapper.EntityToDto);
        }

        #endregion

        #region Création et gestion

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, int userId)
        {
            // Validation : Vérifier que l'utilisateur existe
            User? user = await _userRepository.GetByIdAsync(userId) 
                ?? throw new NotFoundException($"Utilisateur avec l'ID {userId} introuvable.");

            // Validation : Au moins un item dans la commande
            if (dto.Items == null || dto.Items.Count == 0)
            {
                throw new DomainException("La commande doit contenir au moins un billet.");
            }

            decimal totalPrice = 0;
            List<Ticket> ticketsToCreate = new();
            List<Performance> performancesToUpdate = new();

            // === PHASE 2 : VÉRIFICATIONS ET RÉSERVATIONS TEMPORAIRES ===
            foreach (OrderItemDto item in dto.Items)
            {
                Performance? performance = await _performanceRepository.GetByIdAsync(item.PerformanceId)
                    ?? throw new NotFoundException($"Performance avec l'ID {item.PerformanceId} introuvable.");

                if (item.Quantity <= 0)
                {
                    throw new DomainException($"La quantité pour la performance {item.PerformanceId} doit être positive.");
                }

                if (performance.AvailableTickets < item.Quantity)
                {
                    throw new DomainException(
                        $"Pas assez de places pour la performance {item.PerformanceId}. " +
                        $"Disponibles : {performance.AvailableTickets}, Demandées : {item.Quantity}"
                    );
                }

                // Réserver temporairement
                performance.BookTickets(item.Quantity);
                performancesToUpdate.Add(performance);

                // Préparer les tickets (sans les sauvegarder encore)
                for (int i = 0; i < item.Quantity; i++)
                {
                    Ticket ticket = new(
                        unitPrice: performance.UnitPrice,
                        performanceId: performance.PerformanceId
                    );
                    ticketsToCreate.Add(ticket);
                    totalPrice += performance.UnitPrice;
                }
            }

            // === PHASE 3 : TRAITEMENT DU PAIEMENT ===
            // Avec Service de paiment intégré dans l'app
            /*var paymentResult = await _paymentService.ProcessPaymentAsync(
                totalPrice: totalPrice,
                userId: userId
            );

            if (!paymentResult.IsSuccess)
            {
                // Paiement échoué : libérer les places
                foreach (Performance performance in performancesToUpdate)
                {
                    performance.ReleaseTickets(
                        ticketsToCreate.Count(t => t.PerformanceId == performance.PerformanceId)
                    );
                    await _performanceRepository.UpdateAsync(performance);
                }

                throw new DomainException($"Paiement échoué : {paymentResult.ErrorMessage}");
            }
            */


            // === PHASE 3 : CRÉER LA COMMANDE ET TRAITER LE PAIEMENT ===

            // Créer l'order avec statut Pending (pour obtenir un OrderId)
            Order order = new(userId: userId, totalPrice: totalPrice);

            // Sauvegarder l'order pour obtenir son OrderId
            Order? createdOrder = await _orderRepository.AddAsync(order)
                ?? throw new DomainException("Erreur lors de la création de la commande.");

            // Appeler le microservice avec le vrai OrderId
            var paymentResponse = await _paymentHttpService.ProcessPaymentAsync(
                amount: totalPrice,
                currency: "EUR",
                orderId: createdOrder.OrderId.ToString()
            );

            if (paymentResponse == null || paymentResponse.Status != "Succeeded")
            {
                var errorMessage = paymentResponse?.ErrorMessage ?? "Service de paiement indisponible";

                // Marquer la commande comme échouée
                createdOrder.MarkAsFailed();
                await _orderRepository.UpdateAsync(createdOrder);

                // Paiement échoué : libérer les places
                foreach (Performance performance in performancesToUpdate)
                {
                    performance.ReleaseTickets(
                        ticketsToCreate.Count(t => t.PerformanceId == performance.PerformanceId)
                    );
                    await _performanceRepository.UpdateAsync(performance);
                }

                throw new DomainException($"Paiement échoué : {errorMessage}");
            }

            // Paiement réussi : confirmer la commande
            createdOrder.ConfirmPayment(paymentResponse.PaymentIntentId);
            await _orderRepository.UpdateAsync(createdOrder);


            // === PHASE 4 : PAIEMENT RÉUSSI - CRÉER LES TICKETS ===

            // L'order existe déjà et est confirmé, on crée les tickets
            foreach (Ticket ticket in ticketsToCreate)
            {
                ticket.OrderId = createdOrder.OrderId;
                await _ticketRepository.AddAsync(ticket);
            }

            // Recharger la commande avec ses tickets pour le retour
            Order? orderWithTickets = await _orderRepository.GetWithTicketsAsync(createdOrder.OrderId);
            return OrderMapper.EntityToDto(orderWithTickets!);

        }

        public async Task<OrderDto> CancelOrderAsync(int orderId, int currentUserId)
        {
            // Récupérer la commande avec ses tickets
            Order? order = await _orderRepository.GetWithTicketsAsync(orderId) 
                ?? throw new NotFoundException($"Commande avec l'ID {orderId} introuvable.");

            // Vérifier les permissions (seul le propriétaire peut annuler)
            if (order.UserId != currentUserId)
            {
                throw new ForbiddenException("Vous n'avez pas la permission d'annuler cette commande.");
            }

            // Vérifier que la commande est annulable
            if (order.Status == OrderStatus.Cancelled)
            {
                throw new DomainException("Cette commande est déjà annulée.");
            }

            if (order.Status == OrderStatus.Refunded)
            {
                throw new DomainException("Impossible d'annuler une commande remboursée.");
            }

            // Annuler la commande
            order.Cancel();
            await _orderRepository.UpdateAsync(order);

            // Annuler tous les tickets et libérer les places
            IEnumerable<Ticket> tickets = await _ticketRepository.GetByOrderIdAsync(orderId);

            // Grouper les tickets par performance pour libérer en une seule fois
            IEnumerable<IGrouping<int, Ticket>> ticketsByPerformance = tickets.GroupBy(t => t.PerformanceId);

            foreach (IGrouping<int, Ticket> group in ticketsByPerformance)
            {
                int performanceId = group.Key;
                int ticketCount = group.Count();

                // Annuler les tickets
                foreach (Ticket ticket in group)
                {
                    ticket.Cancel();
                    await _ticketRepository.UpdateAsync(ticket);
                }

                // Libérer les places (augmente AvailableTickets, gère retour à Scheduled)
                Performance? performance = await _performanceRepository.GetByIdAsync(performanceId);
                if (performance != null)
                {
                    performance.ReleaseTickets(ticketCount);
                    await _performanceRepository.UpdateAsync(performance);
                }
            }

            return OrderMapper.EntityToDto(order);
        }

        public async Task<OrderDto> ChangeOrderStatusAsync(int orderId, OrderStatus newStatus, int currentUserId, bool isAdmin)
        {
            // Récupérer la commande
            Order? order = await _orderRepository.GetByIdAsync(orderId) 
                ?? throw new NotFoundException($"Commande avec l'ID {orderId} introuvable.");

            // Vérifier les permissions
            // Seul l'admin ou le propriétaire peuvent changer le statut
            if (!isAdmin && order.UserId != currentUserId)
            {
                throw new ForbiddenException("Vous n'avez pas la permission de modifier cette commande.");
            }

            // Vérifier que le changement est valide
            if (order.Status == newStatus)
            {
                throw new DomainException($"La commande est déjà au statut {newStatus}.");
            }

            // Changer le statut selon la transition
            switch (newStatus)
            {
                case OrderStatus.PaymentConfirmed:
                    if (order.Status != OrderStatus.Pending)
                    {
                        throw new DomainException("Seules les commandes en attente peuvent être marquées comme payées.");
                    }
                    order.MarkAsPaid();
                    break;

                case OrderStatus.Cancelled:
                    // Utiliser CancelOrderAsync pour annuler proprement
                    return await CancelOrderAsync(orderId, currentUserId);

                case OrderStatus.Refunded:
                    if (order.Status != OrderStatus.PaymentConfirmed)
                    {
                        throw new DomainException("Seules les commandes payées peuvent être remboursées.");
                    }
                    order.Refund();
                    break;

                case OrderStatus.Pending:
                    throw new DomainException("Impossible de repasser une commande en attente.");

                default:
                    throw new ArgumentException($"Statut invalide : {newStatus}");
            }

            await _orderRepository.UpdateAsync(order);

            return OrderMapper.EntityToDto(order);
        }

        public async Task ConfirmOrderAsync(int orderId, string paymentIntentId)
        {
            // 1. Récupérer la commande avec ses tickets
            Order? order = await _orderRepository.GetWithTicketsAsync(orderId);

            if (order == null)
            {
                throw new NotFoundException($"Order {orderId} not found");
            }

            // 2. Utiliser la méthode métier de l'entité Order
            // Cette méthode va :
            // - Vérifier que Status == Pending
            // - Assigner le PaymentIntentId
            // - Changer le Status en PaymentConfirmed
            // - Mettre à jour UpdatedAt
            order.ConfirmPayment(paymentIntentId);

            // 3. Marquer les tickets comme payés
            // La méthode MarkAsPaid() de Order appelle déjà ticket.MarkAsPaid() sur tous les tickets
            order.MarkAsPaid();

            // 4. Sauvegarder en base de données
            await _orderRepository.UpdateAsync(order);
        }

        #endregion
    }
}
