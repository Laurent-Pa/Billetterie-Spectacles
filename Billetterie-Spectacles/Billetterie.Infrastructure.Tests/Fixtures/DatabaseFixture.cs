using Billetterie_Spectacles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Billetterie_Spectacles.Infrastructure.Tests.Fixtures
{
    public class DatabaseFixture : IDisposable
    {
        private readonly DbContextOptions<BilletterieDbContext> _options;
        private readonly string _connectionString;

        public DatabaseFixture()
        {

            // Charger la configuration depuis appsettings.Test.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false)
                .AddJsonFile("appsettings.Test.Local.json", optional: true) // si besoin d'une config locale, non versionnée
                .Build();

            // Récupérer la connection string
            _connectionString = configuration.GetConnectionString("TestDatabase")
                ?? throw new InvalidOperationException("Connection string 'TestDatabase' not found in appsettings.Test.json");

            // Configurer les options pour SQL Server
            _options = new DbContextOptionsBuilder<BilletterieDbContext>()
                .UseSqlServer(_connectionString)
                .Options;
            // Créer la base de données et appliquer le schéma
            using BilletterieDbContext context = CreateContext();
            context.Database.EnsureDeleted(); // Supprimer si existe déjà (DB propre à chaque exécution)
            context.Database.EnsureCreated(); // Créer avec le schéma complet
        }

        /// <summary>
        /// Crée un nouveau DbContext pour un test.
        /// Chaque test devrait obtenir son propre contexte.
        /// </summary>
        public BilletterieDbContext CreateContext()
        {
            return new BilletterieDbContext(_options);
        }

        /// <summary>
        /// Nettoie toutes les données de la base de test.
        /// À appeler entre les tests pour isoler les données.
        /// </summary>
        public void Cleanup()
        {
            using BilletterieDbContext context = CreateContext();

            // Supprimer dans l'ordre inverse des dépendances (Foreign Keys)
            context.Tickets.RemoveRange(context.Tickets);
            context.Orders.RemoveRange(context.Orders);
            context.Performances.RemoveRange(context.Performances);
            context.Spectacles.RemoveRange(context.Spectacles);
            context.Users.RemoveRange(context.Users);

            context.SaveChanges();
        }

        /// <summary>
        /// Supprime la base de données de test.
        /// Appelé automatiquement par xUnit à la fin de tous les tests de la classe.
        /// </summary>
        public void Dispose()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();

            // Informe le GC qu'il n'a pas besoin d'appeler le finaliseur
            GC.SuppressFinalize(this);

        }
    }
}
