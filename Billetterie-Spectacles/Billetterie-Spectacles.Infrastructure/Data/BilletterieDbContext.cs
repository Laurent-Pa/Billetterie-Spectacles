using Billetterie_Spectacles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billetterie_Spectacles.Infrastructure.Data
{
    public class BilletterieDbContext(DbContextOptions<BilletterieDbContext> options) : DbContext(options)
    {

        // DbSets : Représentent les tables de la base de données
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Spectacle> Spectacles { get; set; } = null!;
        public DbSet<Performance> Performances { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;

        // Configuration du modèle (Fluent API)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des tables
            ConfigureUsers(modelBuilder);
            ConfigureSpectacles(modelBuilder);
            ConfigurePerformances(modelBuilder);
            ConfigureOrders(modelBuilder);
            ConfigureTickets(modelBuilder);
        }

        #region Configuration des entités

        private static void ConfigureUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                // Nom de la table
                entity.ToTable("Users");

                // Clé primaire
                entity.HasKey(u => u.UserId);

                // Propriétés
                entity.Property(u => u.UserId)
                    .HasColumnName("user_id")
                    .ValueGeneratedOnAdd();  // Auto-increment

                entity.Property(u => u.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Surname)
                    .HasColumnName("surname")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.Password)
                    .HasColumnName("password")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(20)
                    .IsRequired(false);  // Nullable

                entity.Property(u => u.Role)
                    .HasColumnName("role")
                    .HasConversion<int>()  // Stocke l'enum comme int
                    .IsRequired();

                entity.Property(u => u.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(u => u.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                // Index unique sur Email
                entity.HasIndex(u => u.Email)
                    .IsUnique();

                // Relations
                entity.HasMany(u => u.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);  // Empêche la suppression en cascade

                entity.HasMany(u => u.CreatedSpectacles)
                    .WithOne(s => s.CreatedByUser)
                    .HasForeignKey(s => s.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureSpectacles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Spectacle>(entity =>
            {
                entity.ToTable("Spectacles");

                entity.HasKey(s => s.SpectacleId);

                entity.Property(s => s.SpectacleId)
                    .HasColumnName("spectacle_id")
                    .ValueGeneratedOnAdd();

                entity.Property(s => s.Name)
                    .HasColumnName("name")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(s => s.Category)
                    .HasColumnName("category")
                    .HasConversion<int>()  // Stocke l'enum comme int
                    .IsRequired();

                entity.Property(s => s.Description)
                    .HasColumnName("description")
                    .HasMaxLength(1000)
                    .IsRequired(false);

                entity.Property(s => s.Duration)
                    .HasColumnName("duration")
                    .IsRequired();

                entity.Property(s => s.Thumbnail)
                    .HasColumnName("thumbnail")
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(s => s.CreatedByUserId)
                    .HasColumnName("created_by_user_id")
                    .IsRequired();

                entity.Property(s => s.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(s => s.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                // Relations
                entity.HasMany(s => s.Performances)
                    .WithOne(p => p.Spectacle)
                    .HasForeignKey(p => p.SpectacleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigurePerformances(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Performance>(entity =>
            {
                entity.ToTable("Performances");

                entity.HasKey(p => p.PerformanceId);

                entity.Property(p => p.PerformanceId)
                    .HasColumnName("performance_id")
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.Date)
                    .HasColumnName("date")
                    .IsRequired();

                entity.Property(p => p.Status)
                    .HasColumnName("status")
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(p => p.UnitPrice)
                    .HasColumnName("unit_price")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(p => p.Capacity)
                    .HasColumnName("capacity")
                    .IsRequired();

                entity.Property(p => p.AvailableTickets)
                    .HasColumnName("available_tickets")
                    .IsRequired();

                entity.Property(p => p.SpectacleId)
                    .HasColumnName("spectacle_id")
                    .IsRequired();

                entity.Property(p => p.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(p => p.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                // Relations
                entity.HasMany(p => p.Tickets)
                    .WithOne(t => t.Performance)
                    .HasForeignKey(t => t.PerformanceId)
                    .OnDelete(DeleteBehavior.Restrict);     // protège les données de ventes (on ne supprime pas une réprésentation s'il y a eu des commandes
            });
        }

        private static void ConfigureOrders(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.HasKey(o => o.OrderId);

                entity.Property(o => o.OrderId)
                    .HasColumnName("order_id")
                    .ValueGeneratedOnAdd();

                entity.Property(o => o.Status)
                    .HasColumnName("status")
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(o => o.TotalPrice)
                    .HasColumnName("total_price")
                    .HasColumnType("decimal(18,2)")  // Précision pour les montants
                    .IsRequired();

                entity.Property(o => o.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(o => o.PaymentIntentId)
                    .HasColumnName("payment_intent_id") 
                    .HasMaxLength(255)
                    .IsRequired(false);  // Nullable

                entity.Property(o => o.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(o => o.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                // === RELATIONS ===

                //  Order -> User
                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                //  Order -> Tickets
                entity.HasMany(o => o.Tickets)
                    .WithOne(t => t.Order)
                    .HasForeignKey(t => t.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureTickets(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Tickets");

                entity.HasKey(t => t.TicketId);

                entity.Property(t => t.TicketId)
                    .HasColumnName("ticket_id")
                    .ValueGeneratedOnAdd();

                entity.Property(t => t.Status)
                    .HasColumnName("status")
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(t => t.UnitPrice)
                    .HasColumnName("unit_price")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(t => t.OrderId)
                    .HasColumnName("order_id")
                    .IsRequired();

                entity.Property(t => t.PerformanceId)
                    .HasColumnName("performance_id")
                    .IsRequired();

                entity.Property(t => t.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(t => t.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                // Relations déjà configurées via Performance et Order (parents)
            });
        }

        #endregion
    }
}