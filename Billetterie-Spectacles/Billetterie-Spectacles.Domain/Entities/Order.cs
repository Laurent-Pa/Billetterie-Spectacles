using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Exceptions;

namespace Billetterie_Spectacles.Domain.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Clé étrangère : Commande passée par un utilisateur (Client)
        public int UserId { get; set; }

        // Propriété de navigation : une commande appartient à un client
        public User User { get; set; } = null!;

        // Relation : Une commande contient plusieurs tickets
        private readonly List<Ticket> _tickets = new();
        public IReadOnlyCollection<Ticket> Tickets => _tickets.AsReadOnly();

        #region Constructors
        private Order() { }         // Constructeur privé pour EF Core

        // Constructeur #1 : Sans totalPrice (pour flux avec AddTicket)
        public Order(int userId) 
        {
            if (userId <= 0)
                throw new ArgumentException("L'ID utilisateur doit être positif.", nameof(userId));

            UserId = userId;
            Status = OrderStatus.Pending;
            Date = DateTime.UtcNow;
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
            Date = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion

        #region Methods
        public void AddTicket(Ticket ticket) // Utilisé par constructeur #1
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("Impossible d'ajouter un ticket à une commande qui n'est pas en attente.");

            if (ticket.Price <= 0)
                throw new ArgumentException("Le prix du ticket doit être positif.", nameof(ticket));

            _tickets.Add(ticket);
            CalculateTotalPrice();
        }

        public void CalculateTotalPrice()
        {
            TotalPrice = _tickets.Sum(t => t.Price);
        }

        /// <summary>
        /// Marque la commande comme payée
        /// </summary>
        public void MarkAsPaid()
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("Seules les commandes en attente peuvent être payées.");

            if (_tickets.Count == 0)
                throw new DomainException("Une commande doit contenir au moins un ticket.");

            Status = OrderStatus.Paid;

            foreach (Ticket ticket in _tickets)
            {
                ticket.MarkAsPaid();
            }

            Status = OrderStatus.Paid;
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
            if (Status != OrderStatus.Paid)
                throw new DomainException("Seules les commandes payées peuvent être remboursées.");

            Status = OrderStatus.Refunded;

            foreach (var ticket in _tickets)
            {
                ticket.Cancel();
            }

            Status = OrderStatus.Refunded;
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion
    }
}
