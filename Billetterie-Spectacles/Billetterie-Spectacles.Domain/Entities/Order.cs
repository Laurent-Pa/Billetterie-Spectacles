using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Exceptions;

namespace Billetterie_Spectacles.Domain.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public string? PaymentIntentId { get; private set; }  // ID Stripe pour le paiement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Clé étrangère : Commande passée par un utilisateur (Client)
        public int UserId { get; set; }

        // Propriété de navigation : une commande appartient à un client
        public User User { get; set; } = null!;

        // Relation : Une commande contient plusieurs tickets

        // Commenté pour tester si readonly pose pb avec EF Core lors de la création d'une commande
        //private readonly List<Ticket> _tickets = new();
        //public IReadOnlyCollection<Ticket> Tickets => _tickets.AsReadOnly();
        public ICollection<Ticket> Tickets { get; private set; } = new List<Ticket>(); // remplace le code commenté juste au-dessus

        #region Constructors
        private Order() { }         // Constructeur privé pour EF Core

        // Constructeur #1 : Sans totalPrice (pour flux avec AddTicket)
        public Order(int userId) 
        {
            if (userId <= 0)
                throw new ArgumentException("L'ID utilisateur doit être positif.", nameof(userId));

            UserId = userId;
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            TotalPrice = 0;
        }

        // Constructeur #2 : Avec totalPrice (pour flux avec calcul préalable)
        public Order(int userId, decimal totalPrice)
        {
            if (userId <= 0)
                throw new ArgumentException("L'ID utilisateur doit être positif.", nameof(userId));

            if (totalPrice < 0)
                throw new ArgumentException("Le prix total ne peut pas être négatif.", nameof(totalPrice));

            UserId = userId;
            TotalPrice = totalPrice;  // ← Prix déjà calculé
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion

        #region Methods
        public void AddTicket(Ticket ticket) // Utilisé par constructeur #1
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("Impossible d'ajouter un ticket à une commande qui n'est pas en attente.");

            if (ticket.UnitPrice <= 0)
                throw new ArgumentException("Le prix du ticket doit être positif.", nameof(ticket));

            Tickets.Add(ticket);
            CalculateTotalPrice();
        }

        public void CalculateTotalPrice()
        {
            TotalPrice = Tickets.Sum(t => t.UnitPrice);
        }

        /// <summary>
        /// Marque la commande comme payée
        /// </summary>
        public void MarkAsPaid()
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("Seules les commandes en attente peuvent être payées.");

            if (Tickets.Count == 0)
                throw new DomainException("Une commande doit contenir au moins un ticket.");

            Status = OrderStatus.PaymentConfirmed;

            foreach (Ticket ticket in Tickets)
            {
                ticket.MarkAsPaid();
            }

            Status = OrderStatus.PaymentConfirmed;
            UpdatedAt = DateTime.UtcNow;
        }


        // <summary>
        /// Annule la commande
        /// </summary>
        public void Cancel()
        {
            if (Status == OrderStatus.Cancelled)
                throw new DomainException("La commande est déjà annulée.");

            if (Status == OrderStatus.Refunded)
                throw new DomainException("Impossible d'annuler une commande remboursée.");

            Status = OrderStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }


        /// <summary>
        /// Rembourse la commande
        /// </summary>
        public void Refund()
        {
            if (Status != OrderStatus.PaymentConfirmed)
                throw new DomainException("Seules les commandes payées peuvent être remboursées.");

            Status = OrderStatus.Refunded;

            foreach (var ticket in Tickets)
            {
                ticket.Cancel();
            }

            Status = OrderStatus.Refunded;
            UpdatedAt = DateTime.UtcNow;
        }

        // Méthode pour confirmer le paiement
        public void ConfirmPayment(string paymentIntentId)
        {
            if (Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Seule une commande en attente peut être confirmée");
            }

            PaymentIntentId = paymentIntentId;
            Status = OrderStatus.PaymentConfirmed;
            UpdatedAt = DateTime.Now;
        }

        // Méthode pour marquer comme échoué
        public void MarkAsFailed()
        {
            Status = OrderStatus.PaymentFailed;
            UpdatedAt = DateTime.Now;
        }
        #endregion
    }
}
