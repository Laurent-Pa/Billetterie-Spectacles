using Billeterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Microsoft.AspNetCore.Identity; // AJOUT : Pour le PasswordHasher

namespace Billetterie_Spectacles.Infrastructure.Data
{
    /// <summary>
    /// Classe pour initialiser la base de données avec des données de test
    /// Permet d'avoir des données cohérentes entre développeurs
    /// </summary>
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Seeding par défaut pour les environnements de développement / démo.
        /// </summary>
        public static void Seed(BilletterieDbContext context)
        {
            // Vérifier si la base contient déjà des données
            if (context.Users.Any())
            {
                Console.WriteLine("Base de données déjà seedée, skip...");
                return; // Ne pas re-seeder si des données existent
            }

            Console.WriteLine("Début du seeding de la base de données (mode standard)...");

            var passwordHasher = new PasswordHasher<User>();

            // === USERS ===
            var adminUser = new User(
                name: "Admin",
                surname: "System",
                email: "admin@billetterie.com",
                password: "admin@test1",
                role: UserRole.Admin
            );
            adminUser.Password = passwordHasher.HashPassword(adminUser, "TestAdmin123!");
            adminUser.ChangeRole(UserRole.Admin);

            var organizerUser = new User(
                name: "Jean",
                surname: "Dupont",
                email: "organizer@billetterie.com",
                password: "organizer@test1",
                role: UserRole.Organizer
            );
            organizerUser.Password = passwordHasher.HashPassword(organizerUser, "TestOrganizer123!");
            organizerUser.ChangeRole(UserRole.Organizer);

            var clientUser = new User(
                name: "Marie",
                surname: "Martin",
                email: "client@billetterie.com",
                password: "client@test1",
                role: UserRole.Client
            );
            clientUser.Password = passwordHasher.HashPassword(clientUser, "TestClient123!");

            context.Users.AddRange(adminUser, organizerUser, clientUser);
            context.SaveChanges();
            Console.WriteLine("3 utilisateurs créés avec rôles corrects");

            // === SPECTACLES ===
            var hamletSpectacle = new Spectacle(
                name: "Hamlet",
                category: SpectacleCategory.Theatre,
                description: "Tragédie de William Shakespeare mettant en scène le prince du Danemark",
                duration: 180,
                createdByUserId: organizerUser.UserId
            );

            var nutcrackerSpectacle = new Spectacle(
                name: "Casse-Noisette",
                category: SpectacleCategory.Danse,
                description: "Ballet classique de Tchaïkovski présenté par le Ballet National",
                duration: 120,
                createdByUserId: organizerUser.UserId
            );

            var jazzConcertSpectacle = new Spectacle(
                name: "Soirée Jazz",
                category: SpectacleCategory.Concert,
                description: "Concert de jazz avec quartet exceptionnel",
                duration: 150,
                createdByUserId: organizerUser.UserId
            );

            context.Spectacles.AddRange(hamletSpectacle, nutcrackerSpectacle, jazzConcertSpectacle);
            context.SaveChanges();
            Console.WriteLine("3 spectacles créés");

            // === PERFORMANCES ===
            var today = DateTime.UtcNow.Date;

            // Performances pour Hamlet
            var hamletPerf1 = new Performance(
                spectacleId: hamletSpectacle.SpectacleId,
                date: today.AddDays(7).AddHours(20),
                capacity: 200,
                unitPrice: 45.00m
            );

            var hamletPerf2 = new Performance(
                spectacleId: hamletSpectacle.SpectacleId,
                date: today.AddDays(14).AddHours(20),
                capacity: 200,
                unitPrice: 45.00m
            );

            // Performances pour Casse-Noisette
            var nutcrackerPerf1 = new Performance(
                spectacleId: nutcrackerSpectacle.SpectacleId,
                date: today.AddDays(10).AddHours(15),
                capacity: 150,
                unitPrice: 55.00m
            );

            var nutcrackerPerf2 = new Performance(
                spectacleId: nutcrackerSpectacle.SpectacleId,
                date: today.AddDays(17).AddHours(15),
                capacity: 150,
                unitPrice: 55.00m
            );

            // Performances pour Soirée Jazz
            var jazzPerf1 = new Performance(
                spectacleId: jazzConcertSpectacle.SpectacleId,
                date: today.AddDays(5).AddHours(21),
                capacity: 100,
                unitPrice: 35.00m
            );

            var jazzPerf2 = new Performance(
                spectacleId: jazzConcertSpectacle.SpectacleId,
                date: today.AddDays(12).AddHours(21),
                capacity: 100,
                unitPrice: 35.00m
            );

            context.Performances.AddRange(
                hamletPerf1, hamletPerf2,
                nutcrackerPerf1, nutcrackerPerf2,
                jazzPerf1, jazzPerf2
            );
            context.SaveChanges();
            Console.WriteLine("6 performances créées");

            Console.WriteLine("Seeding terminé avec succès (mode standard) !");
            Console.WriteLine();
            Console.WriteLine("=== Comptes de test disponibles ===");
            Console.WriteLine("   Admin      → admin@billetterie.com / TestAdmin123!");
            Console.WriteLine("   Organizer  → organizer@billetterie.com / TestOrganizer123!");
            Console.WriteLine("   Client     → client@billetterie.com / TestClient123!");
        }

        /// <summary>
        /// Seeding spécifique pour l'environnement "Testing" (Selenium).
        /// Utilise une base SQLite séparée et un jeu de données minimal.
        /// </summary>
        public static void SeedForTesting(BilletterieDbContext context)
        {
            Console.WriteLine("Début du seeding de la base de données (mode Testing / Selenium)...");

            // Nettoyer les données existantes pour garantir un état déterministe
            context.Tickets.RemoveRange(context.Tickets);
            context.Orders.RemoveRange(context.Orders);
            context.Performances.RemoveRange(context.Performances);
            context.Spectacles.RemoveRange(context.Spectacles);
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            var passwordHasher = new PasswordHasher<User>();

            // === USERS (seulement ceux demandés pour les tests E2E) ===
            var adminUser = new User(
                name: "Admin",
                surname: "System",
                email: "admin@billetterie.com",
                password: "admin@test1",
                role: UserRole.Admin
            );
            adminUser.Password = passwordHasher.HashPassword(adminUser, "TestAdmin123!");
            adminUser.ChangeRole(UserRole.Admin);

            var testClientUser = new User(
                name: "Test",
                surname: "User",
                email: "test@billetterie.com",
                password: "test@test1",
                role: UserRole.Client
            );
            testClientUser.Password = passwordHasher.HashPassword(testClientUser, "TestTest123!");

            context.Users.AddRange(adminUser, testClientUser);
            context.SaveChanges();
            Console.WriteLine("Utilisateurs de test créés (admin + client).");

            // === SPECTACLES de test ===
            var today = DateTime.UtcNow.Date;

            var testTheatre = new Spectacle(
                name: "Spectacle Selenium - Théâtre",
                category: SpectacleCategory.Theatre,
                duration: 120,
                createdByUserId: adminUser.UserId,
                description: "Spectacle de test (Théâtre) pour les scénarios E2E Selenium.",
                thumbnail: null
            );

            var testConcert = new Spectacle(
                name: "Spectacle Selenium - Concert",
                category: SpectacleCategory.Concert,
                duration: 90,
                createdByUserId: adminUser.UserId,
                description: "Concert de test pour les scénarios E2E Selenium.",
                thumbnail: null
            );

            context.Spectacles.AddRange(testTheatre, testConcert);
            context.SaveChanges();
            Console.WriteLine("Spectacles de test créés.");

            // === PERFORMANCES de test ===
            var theatrePerf = new Performance(
                spectacleId: testTheatre.SpectacleId,
                date: today.AddDays(3).AddHours(20),
                capacity: 50,
                unitPrice: 25.00m
            );

            var concertPerf = new Performance(
                spectacleId: testConcert.SpectacleId,
                date: today.AddDays(5).AddHours(21),
                capacity: 80,
                unitPrice: 35.00m
            );

            context.Performances.AddRange(theatrePerf, concertPerf);
            context.SaveChanges();
            Console.WriteLine("Performances de test créées.");

            Console.WriteLine("Seeding terminé avec succès (mode Testing).");
            Console.WriteLine();
            Console.WriteLine("=== Comptes Selenium disponibles ===");
            Console.WriteLine("   Admin  → admin@billetterie.com / TestAdmin123!");
            Console.WriteLine("   Client → test@billetterie.com / TestTest123!");
        }
    }
}