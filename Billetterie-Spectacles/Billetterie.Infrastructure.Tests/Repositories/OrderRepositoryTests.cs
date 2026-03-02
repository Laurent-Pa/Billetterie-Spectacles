using Billeterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Infrastructure.Data;
using Billetterie_Spectacles.Infrastructure.Repositories;
using Billetterie_Spectacles.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;


namespace Billetterie_Spectacles.Infrastructure.Tests.Repositories
{
    public class OrderRepositoryTests(DatabaseFixture databaseFixture) : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture = databaseFixture;

#region Create and retrieve a simple order
        [Fact]
        public async Task AddAsync_ValidOrder_ShouldSaveToDataBase()
        {

            // --- ARRANGE ---
            _fixture.Cleanup();

            using BilletterieDbContext context = _fixture.CreateContext();
            OrderRepository repository = new(context);

            // Créer un utilisateur avec le constructeur
            User user = new
                (
                name: "John",
                surname: "Doe",
                email: "john.doe@test.com",
                password: "TestP@ssw0rd123",
                phone: "0612345678",
                role: UserRole.Client
                );

            context.Users.Add( user );
            await context.SaveChangesAsync(TestContext.Current.CancellationToken); // modif suggéré par VS

            // Créer une commande
            Order order = new(
                userId: user.UserId
                // Status: OrderStatus.Pending  --> paramètre hors constructeur (valeur par défaut)
                // totalPrice: 0                --> paramètre hors constructeur (valeur par défaut)
                );


            // --- ACT ---

            // Ajouter la commande dans le repository
            await repository.AddAsync(order);
            // xUnit.v3 permet d'annuler les tests en timeout avec le CancellationToken
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);


            // --- ASSERT ---
            using BilletterieDbContext verificationContext = _fixture.CreateContext();
            Order? savedOrder = await verificationContext.Orders.FindAsync(order.OrderId);

            savedOrder.ShouldNotBeNull();
            savedOrder.UserId.ShouldBe(user.UserId);
            savedOrder.Status.ShouldBe(OrderStatus.Pending);
            savedOrder.TotalPrice.ShouldBe(0m);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingOrder_ShouldReturnOrder()
        {
            // --- ARRANGE ---
            _fixture.Cleanup();

            using BilletterieDbContext context = _fixture.CreateContext();
            OrderRepository repository = new(context);

            // Créer un utilisateur avec le constructeur
            User user = new
                (
                name: "John",
                surname: "Doe",
                email: "john.doe@test.com",
                password: "TestP@ssw0rd123",
                phone: "0612345678",
                role: UserRole.Client
                );

            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Créer et sauvegarder une commande
            var order = new Order(userId: user.UserId);
            await repository.AddAsync(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // --- ACT ---
            // Récupérer la commande par son id
            Order? retrievedOrder = await repository.GetByIdAsync(order.OrderId);

            // --- ASSERT ---
            retrievedOrder.ShouldNotBeNull();
            retrievedOrder.OrderId.ShouldBe(order.OrderId);
            retrievedOrder.Status.ShouldBe(OrderStatus.Pending);
            retrievedOrder.UserId.ShouldBe(user.UserId);
            retrievedOrder.TotalPrice.ShouldBe(0m);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingOrder_ShouldReturnNull()
        {
            // --- ARRANGE ---
            _fixture.Cleanup();

            using BilletterieDbContext context = _fixture.CreateContext();
            OrderRepository repository = new (context);

            int nonExistingOrderId = 99999;

            // --- ACT ---
            Order? retrievedOrder = await repository.GetByIdAsync(nonExistingOrderId);

            // --- ASSERT ---
            retrievedOrder.ShouldBeNull(); // Ne dois pas retourner une exception (404)
        }
        #endregion

        #region Relation Order + Ticket

        [Fact]
        public async Task AddAsync_OrderWithTickets_ShouldSaveOrderandTickets()
        {
            // --- ARRANGE ---
            _fixture.Cleanup();

            using BilletterieDbContext context = _fixture.CreateContext();
            OrderRepository repository = new(context);

            // Créer un utilisateur avec le constructeur
            User user = new
                (
                name: "John",
                surname: "Doe",
                email: "john.doe@test.com",
                password: "TestP@ssw0rd123",
                phone: "0612345678",
                role: UserRole.Client
                );

            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Créer un spectacle 
            Spectacle spectacle = new(
                name: "Le lac des cygnes",
                category: SpectacleCategory.Danse,
                description: "Ballet classique",
                duration: 120,
                createdByUserId: user.UserId
                );
            context.Spectacles.Add(spectacle);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Créer une performance correspondant au spectacle
            Performance performance = new(
                spectacleId: spectacle.SpectacleId,
                date: DateTime.UtcNow.AddDays(30),
                unitPrice: 45.0m,
                capacity: 200
                );
            context.Performances.Add(performance);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Créer une commande
            Order order = new(userId: user.UserId);

            // Créer deux tickets
            Ticket ticket1 = new (performanceId: performance.PerformanceId, unitPrice:performance.UnitPrice);
            Ticket ticket2 = new (performanceId: performance.PerformanceId, unitPrice:performance.UnitPrice);

            order.AddTicket(ticket1);
            order.AddTicket(ticket2);

            // --- ACT ---
            await repository.AddAsync(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Vérification avec un nouveau contexte
            using BilletterieDbContext verificationContext = _fixture.CreateContext();
            OrderRepository verificationRepository = new (verificationContext);
            Order? savedOrder = await verificationRepository.GetWithTicketsAsync(order.OrderId);

            // Vérifier que les tickets existent bien en DB
            List<Ticket> savedTickets = verificationContext.Tickets
                .Where(t => t.OrderId == order.OrderId)
                .ToList();


            // --- ASSERT ---

            savedOrder.ShouldNotBeNull();
            savedOrder.Tickets.Count.ShouldBe(2);
            savedOrder.TotalPrice.ShouldBe(90.0m);

            savedTickets.ShouldNotBeNull();
            savedTickets.Count.ShouldBe(2);
            savedTickets.All(t => t.PerformanceId == performance.PerformanceId).ShouldBeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_WithInclude_ShouldLoadTickets()
        {
            // --- ARRANGE ---
            _fixture.Cleanup();

            using BilletterieDbContext context = _fixture.CreateContext();
            OrderRepository repository = new(context);

            // Créer un utilisateur avec le constructeur
            User user = new
                (
                name: "John",
                surname: "Doe",
                email: "john.doe@test.com",
                password: "TestP@ssw0rd123",
                phone: "0612345678",
                role: UserRole.Client
                );

            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Créer un spectacle 
            Spectacle spectacle = new(
                name: "Le lac des cygnes",
                category: SpectacleCategory.Danse,
                description: "Ballet classique",
                duration: 120,
                createdByUserId: user.UserId
                );
            context.Spectacles.Add(spectacle);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Créer une performance correspondant au spectacle
            Performance performance = new(
                spectacleId: spectacle.SpectacleId,
                date: DateTime.UtcNow.AddDays(30),
                unitPrice: 45.0m,
                capacity: 200
                );
            context.Performances.Add(performance);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Créer une commande
            Order order = new(userId: user.UserId);

            // Créer deux tickets
            Ticket ticket1 = new(performanceId: performance.PerformanceId, unitPrice: performance.UnitPrice);
            Ticket ticket2 = new(performanceId: performance.PerformanceId, unitPrice: performance.UnitPrice);

            order.AddTicket(ticket1);
            order.AddTicket(ticket2);

            // --- ACT ---
            await repository.AddAsync(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

           // Vérification avec un nouveau contexte
            using BilletterieDbContext retrievalContext = _fixture.CreateContext();
            OrderRepository retrievalRepository = new (retrievalContext);
            Order? retrievedOrder = await retrievalRepository.GetWithTicketsAsync(order.OrderId);

            // --- ASSERT ---
            retrievedOrder.ShouldNotBeNull();
            retrievedOrder.Tickets.ShouldNotBeNull();
            retrievedOrder.Tickets.Count.ShouldBe(2);

            // Vérifier que les tickets sont bien chargés (/!\ pas de lazy loading)
            retrievedOrder.Tickets.All(t => t.PerformanceId == performance.PerformanceId).ShouldBeTrue();
        }
        #endregion
    }
}
