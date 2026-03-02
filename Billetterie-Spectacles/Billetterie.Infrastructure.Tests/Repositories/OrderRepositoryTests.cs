using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Infrastructure.Data;
using Billetterie_Spectacles.Infrastructure.Repositories;
using Billetterie_Spectacles.Infrastructure.Tests.Fixtures;
using Shouldly;


namespace Billetterie_Spectacles.Infrastructure.Tests.Repositories
{
    public class OrderRepositoryTests(DatabaseFixture databaseFixture) : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture = databaseFixture;

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
    }
}
