using Billetterie_Spectacles.Domain.Entities;  // Pour accéder à Order
using Shouldly;                                 // Pour les assertions lisibles
namespace Billetterie_Spectacles.Domain.Tests.Entities
{
    public class OrderTests
    {
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
    }
}
