using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Exceptions;

namespace Billetterie_Spectacles.Domain.Entities
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public TicketStatus Status { get; private set; } = TicketStatus.Reserved;
        public decimal Price { get; set; }

        // Clés étrangères
        public int OrderId { get; set; }
        public int PerformanceId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propriétés de navigation pour faciliter les requetes avec EF Core
        public Order Order { get; set; } = null!;
        public Performance Performance { get; set; } = null!;

        #region Constructor

        private Ticket() { }

        public Ticket(int performanceId, decimal price)
        {
            if (performanceId <= 0)
                throw new ArgumentException("L'ID de la performance doit être positif.", nameof(performanceId));

            if (price <= 0)
                throw new ArgumentException("Le prix doit être strictement positif.", nameof(price));

            PerformanceId = performanceId;
            Price = price;
            Status = TicketStatus.Reserved;
        }
        #endregion

        #region Methods
        public void MarkAsPaid()
        {
            if (Status != TicketStatus.Reserved)
                throw new DomainException("Seul un ticket réservé peut être marqué comme payé.");

            Status = TicketStatus.Paid;
        }

        public void MarkAsUsed()
        {
            if (Status != TicketStatus.Paid)
                throw new DomainException("Seul un ticket payé peut être marqué comme utilisé.");

            Status = TicketStatus.Used;
        }

        public void Cancel()
        {
            if (Status == TicketStatus.Used)
                throw new DomainException("Un ticket déjà utilisé ne peut pas être annulé.");

            Status = TicketStatus.Cancelled;
        }
        #endregion
    }
}
