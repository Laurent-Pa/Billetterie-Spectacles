using Billetterie_Spectacles.Domain.Entities;
using Billetterie_Spectacles.Domain.Enums; // Pour UserRole
using Billeterie_Spectacles.Domain.Enums; // Pour SpectacleCategory
using Microsoft.EntityFrameworkCore;

namespace Billetterie_Spectacles.Infrastructure.Data
{
    /// <summary>
    /// Service de seeding pour initialiser la base de données avec des données de test
    /// </summary>
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Initialise la base de données avec des données de test
        /// </summary>
        public static async Task SeedAsync(BilletterieDbContext context)
        {
            // Vérifier si la base de données existe déjà
            await context.Database.EnsureCreatedAsync();

            // Vérifier si un admin existe déjà, sinon le créer
            User? admin = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
            if (admin == null)
            {
                // Créer le compte admin
                admin = new User(
                    name: "Admin",
                    surname: "Système",
                    email: "admin@billetterie.com",
                    password: BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    role: UserRole.Client, // Par défaut, sera changé après
                    phone: "+33612345678"
                );
                admin.ChangeRole(UserRole.Admin); // Changer le rôle en Admin
                admin.CreatedAt = DateTime.UtcNow;
                admin.UpdatedAt = DateTime.UtcNow;

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
            }

            // Vérifier si des spectacles existent déjà
            if (await context.Spectacles.AnyAsync())
            {
                // Les spectacles existent déjà
                return;
            }

            // Créer 20 spectacles variés
            var spectacles = new List<Spectacle>
            {
                new Spectacle("Roméo et Juliette", SpectacleCategory.Theatre, 120, admin.UserId, 
                    "La tragique histoire d'amour de Shakespeare", "https://example.com/romeo.jpg"),
                new Spectacle("Le Misanthrope", SpectacleCategory.Theatre, 150, admin.UserId,
                    "Comédie de Molière sur l'hypocrisie sociale", "https://example.com/misanthrope.jpg"),
                new Spectacle("Antigone", SpectacleCategory.Theatre, 90, admin.UserId,
                    "Tragédie grecque de Sophocle", "https://example.com/antigone.jpg"),
                new Spectacle("Cyrano de Bergerac", SpectacleCategory.Theatre, 180, admin.UserId,
                    "Drame héroïque d'Edmond Rostand", "https://example.com/cyrano.jpg"),
                new Spectacle("L'Avare", SpectacleCategory.Theatre, 110, admin.UserId,
                    "Comédie de Molière sur l'avarice", "https://example.com/avare.jpg"),
                new Spectacle("Concert Symphonique Classique", SpectacleCategory.Concert, 120, admin.UserId,
                    "Orchestre philharmonique interprétant les plus grands classiques", "https://example.com/symphonie.jpg"),
                new Spectacle("Jazz Night", SpectacleCategory.Concert, 180, admin.UserId,
                    "Soirée jazz avec des musiciens de renom", "https://example.com/jazz.jpg"),
                new Spectacle("Rock Legends", SpectacleCategory.Concert, 150, admin.UserId,
                    "Hommage aux légendes du rock", "https://example.com/rock.jpg"),
                new Spectacle("Opéra La Traviata", SpectacleCategory.Concert, 200, admin.UserId,
                    "Opéra de Verdi interprété par la troupe nationale", "https://example.com/traviata.jpg"),
                new Spectacle("Concert de Musique Électronique", SpectacleCategory.Concert, 240, admin.UserId,
                    "Festival de musique électronique avec DJ internationaux", "https://example.com/electro.jpg"),
                new Spectacle("Le Lac des Cygnes", SpectacleCategory.Danse, 120, admin.UserId,
                    "Ballet classique de Tchaïkovski", "https://example.com/cygnes.jpg"),
                new Spectacle("Casse-Noisette", SpectacleCategory.Danse, 100, admin.UserId,
                    "Ballet féerique de Tchaïkovski", "https://example.com/cassenoisette.jpg"),
                new Spectacle("Danse Contemporaine Moderne", SpectacleCategory.Danse, 90, admin.UserId,
                    "Spectacle de danse contemporaine innovant", "https://example.com/contemporain.jpg"),
                new Spectacle("Flamenco Passion", SpectacleCategory.Danse, 110, admin.UserId,
                    "Spectacle de flamenco avec danseurs espagnols", "https://example.com/flamenco.jpg"),
                new Spectacle("Hip-Hop Battle", SpectacleCategory.Danse, 120, admin.UserId,
                    "Compétition de danse hip-hop", "https://example.com/hiphop.jpg"),
                new Spectacle("Les Fourberies de Scapin", SpectacleCategory.Theatre, 100, admin.UserId,
                    "Comédie de Molière", "https://example.com/scapin.jpg"),
                new Spectacle("Hamlet", SpectacleCategory.Theatre, 200, admin.UserId,
                    "Tragédie de Shakespeare", "https://example.com/hamlet.jpg"),
                new Spectacle("Concert de Piano Solo", SpectacleCategory.Concert, 90, admin.UserId,
                    "Récital de piano par un virtuose", "https://example.com/piano.jpg"),
                new Spectacle("Ballet Giselle", SpectacleCategory.Danse, 130, admin.UserId,
                    "Ballet romantique classique", "https://example.com/giselle.jpg"),
                new Spectacle("Théâtre d'Improvisation", SpectacleCategory.Theatre, 90, admin.UserId,
                    "Spectacle interactif d'improvisation théâtrale", "https://example.com/impro.jpg")
            };

            foreach (var spectacle in spectacles)
            {
                spectacle.CreatedAt = DateTime.UtcNow;
                spectacle.UpdatedAt = DateTime.UtcNow;
            }

            await context.Spectacles.AddRangeAsync(spectacles);
            await context.SaveChangesAsync();

            // Créer des représentations pour chaque spectacle
            var random = new Random();
            var performances = new List<Performance>();

            foreach (var spectacle in spectacles)
            {
                // Créer 2-4 représentations par spectacle
                int numberOfPerformances = random.Next(2, 5);
                
                for (int i = 0; i < numberOfPerformances; i++)
                {
                    // Date dans le futur (entre 7 et 90 jours)
                    DateTime performanceDate = DateTime.UtcNow.AddDays(random.Next(7, 91));
                    
                    // Capacité entre 50 et 500 places
                    int capacity = random.Next(50, 501);
                    
                    // Prix entre 15€ et 150€
                    decimal price = (decimal)(random.Next(1500, 15001) / 100.0);
                    
                    var performance = new Performance(
                        spectacleId: spectacle.SpectacleId,
                        date: performanceDate,
                        capacity: capacity,
                        unitPrice: price
                    );
                    
                    performance.CreatedAt = DateTime.UtcNow;
                    performance.UpdatedAt = DateTime.UtcNow;
                    
                    performances.Add(performance);
                }
            }

            await context.Performances.AddRangeAsync(performances);
            await context.SaveChangesAsync();
        }
    }
}
