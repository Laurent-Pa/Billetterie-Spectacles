using Billeterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // AJOUT : Pour le PasswordHasher

namespace Billetterie_Spectacles.Infrastructure.Data
{
    /// <summary>
    /// Classe pour initialiser la base de données avec des données de test
    /// Permet d'avoir des données cohérentes entre développeurs
    /// </summary>
    public static class DatabaseSeeder
    {
        public static void Seed(BilletterieDbContext context)
        {
            // Vérifier si la base contient déjà des données
            if (context.Users.Any())
            {
                Console.WriteLine("Base de données déjà seedée, skip...");
                return; // Ne pas re-seeder si des données existent
            }

            Console.WriteLine("Début du seeding de la base de données...");

            // AJOUT : Créer une instance du PasswordHasher pour hacher les mots de passe
            var passwordHasher = new PasswordHasher<User>();

            // === USERS ===
            var adminUser = new User(
                name: "Admin",
                surname: "System",
                email: "admin@billetterie.com",
                password: "admin@test1", // Temporaire, sera remplacé juste après
                role: UserRole.Admin
            );
            // Hacher le mot de passe
            adminUser.Password = passwordHasher.HashPassword(adminUser, "TestAdmin123!");

            var organizerUser = new User(
                name: "Jean",
                surname: "Dupont",
                email: "organizer@billetterie.com",
                password: "organizer@test1",
                role: UserRole.Organizer
            );
            organizerUser.Password = passwordHasher.HashPassword(organizerUser, "TestOrganizer123!");

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
            Console.WriteLine("3 utilisateurs créés avec mots de passe hachés");

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
                date: today.AddDays(7).AddHours(20), // Dans 7 jours à 20h
                capacity: 200,
                unitPrice: 45.00m
            );

            var hamletPerf2 = new Performance(
                spectacleId: hamletSpectacle.SpectacleId,
                date: today.AddDays(14).AddHours(20), // Dans 14 jours à 20h
                capacity: 200,
                unitPrice: 45.00m
            );

            // Performances pour Casse-Noisette
            var nutcrackerPerf1 = new Performance(
                spectacleId: nutcrackerSpectacle.SpectacleId,
                date: today.AddDays(10).AddHours(15), // Dans 10 jours à 15h
                capacity: 150,
                unitPrice: 55.00m
            );

            var nutcrackerPerf2 = new Performance(
                spectacleId: nutcrackerSpectacle.SpectacleId,
                date: today.AddDays(17).AddHours(15), // Dans 17 jours à 15h
                capacity: 150,
                unitPrice: 55.00m
            );

            // Performances pour Soirée Jazz
            var jazzPerf1 = new Performance(
                spectacleId: jazzConcertSpectacle.SpectacleId,
                date: today.AddDays(5).AddHours(21), // Dans 5 jours à 21h
                capacity: 100,
                unitPrice: 35.00m
            );

            var jazzPerf2 = new Performance(
                spectacleId: jazzConcertSpectacle.SpectacleId,
                date: today.AddDays(12).AddHours(21), // Dans 12 jours à 21h
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

            Console.WriteLine("Seeding terminé avec succès !");
            Console.WriteLine();
            Console.WriteLine("=== Comptes de test disponibles ===");
            Console.WriteLine("   Admin      → admin@billetterie.com / TestAdmin123!");
            Console.WriteLine("   Organizer  → organizer@billetterie.com / TestOrganizer123!");
            Console.WriteLine("   Client     → client@billetterie.com / TestClient123!");
        }
    }
}