using Billetterie_Spectacles.Domain.Entities;  // Pour accéder à Order
using Billetterie_Spectacles.Domain.Exceptions;
using Shouldly;                                 // Pour les assertions lisibles
namespace Billetterie_Spectacles.Domain.Tests.Entities
{
    public class OrderTests
    {
        #region Success Cases
        [Fact]
        public void AddTicket_ValidTicket_WhenOrderIsPending_ShouldAddToTicketsCollection()
        {
            // ARRANGE - Préparer les objets de test
            Order order = new(userId: 1);
            Ticket ticket = new(performanceId: 1, unitPrice: 25.50m);

            // ACT - Exécuter la méthode à tester
            order.AddTicket(ticket);

            // ASSERT - Vérifier le résultat
            order.Tickets.ShouldContain(ticket);
            order.Tickets.Count.ShouldBe(1);
        }

        [Fact]
        public void AddTicket_ValidTicket_WhenOrderIsPending_ShouldRecalculateTotalPrice()
        {
            // ARRANGE
            Order order = new (userId: 1);
            Ticket ticket = new (performanceId: 1, unitPrice: 25.50m);

            // Vérification initiale : TotalPrice devrait être 0 AVANT qu'on ajoute un ticket
            order.TotalPrice.ShouldBe(0m);

            // ACT
            order.AddTicket(ticket);

            // ASSERT: vérification finale : TotalPrice doit être égal au prix APRES qu'on ajoute un ticket
            order.TotalPrice.ShouldBe(25.50m);
        }

        [Fact]
        public void AddTicket_MultipleTickets_ShouldCalculateTotalPriceCorrectly()
        {
            // ARRANGE
            Order order = new (userId: 1);
            Ticket ticket1 = new (performanceId: 1, unitPrice: 25.00m);
            Ticket ticket2 = new (performanceId: 1, unitPrice: 25.00m);
            Ticket ticket3 = new (performanceId: 2, unitPrice: 15.00m);

            // ACT
            order.AddTicket(ticket1);
            order.AddTicket(ticket2);
            order.AddTicket(ticket3);

            // ASSERT
            order.TotalPrice.ShouldBe(65.00m); // 25.00 + 25.00 + 15.00
            order.Tickets.Count.ShouldBe(3);
        }
        #endregion

        #region Status Validation

        [Fact]
        public void AddTicket_WhenOrderStatusIsPaymentFailed_ShouldThrowDomainException()
        {
            // ARRANGE
            Order order = new (userId: 1);
            order.MarkAsFailed(); 

            Ticket ticket = new (performanceId: 1, unitPrice: 25.50m);

            // ACT & ASSERT
            var exception = Should.Throw<DomainException>(() =>
            {
                order.AddTicket(ticket);
            });

            exception.Message.ShouldBe("Impossible d'ajouter un ticket à une commande qui n'est pas en attente.");
        }

        [Fact]
        public void AddTicket_WhenOrderStatusIsCancelled_ShouldThrowDomainException()
        {
            // ARRANGE
            Order order = new (userId: 1);
            order.Cancel();

            Ticket ticket = new (performanceId: 1, unitPrice: 25.50m);

            // ACT & ASSERT
            var exception = Should.Throw<DomainException>(() =>
            {
                order.AddTicket(ticket);
            });

            exception.Message.ShouldBe("Impossible d'ajouter un ticket à une commande qui n'est pas en attente.");
        }

        [Fact]
        public void AddTicket_WhenOrderStatusIsPaymentConfirmed_ShouldThrowDomainException()
        {
            // ARRANGE
            Order order = new (userId: 1);
            order.ConfirmPayment("payment_intent"); // Change à PaymentConfirmed sans vérifier les tickets

            Ticket ticket = new (performanceId: 1, unitPrice: 25.50m);

            // ACT & ASSERT
            var exception = Should.Throw<DomainException>(() =>
            {
                order.AddTicket(ticket);
            });

            exception.Message.ShouldBe("Impossible d'ajouter un ticket à une commande qui n'est pas en attente.");
        }
        #endregion
    }
}
