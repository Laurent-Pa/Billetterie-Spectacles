using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Exceptions;

namespace Billetterie_Spectacles.Domain.Entities
{
    public class Performance
    {
        public int PerformanceId { get; set; }
        public DateTime Date { get; set; }
        public PerformanceStatus Status { get; private set; } = PerformanceStatus.Scheduled;
        public int Capacity { get; set; }
        public int AvailableTickets { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Clé étrangère : Performance liée à un spectacle
        public int SpectacleId { get; set; }

        // Propriété de navigation : Performance appartient à un spectacle
        public Spectacle Spectacle { get; set; } = null!;

        // Relation : Une performance génère plusieurs tickets
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        #region Constructors
        private Performance() { }

        public Performance(int spectacleId, DateTime date, int capacity)
        {
            ValidateSpectacleId(spectacleId);
            ValidateDate(date);
            ValidateCapacity(capacity);

            SpectacleId = spectacleId;
            Date = date;
            Capacity = capacity;
            AvailableTickets = capacity; // A l'initialisation il n'y a pas de tickets vendus
            Status = PerformanceStatus.Scheduled;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion

        #region Methods
        // Methodes métier
        public void BookTickets(int quantity)
        {
            if (Status != PerformanceStatus.Scheduled)
                throw new DomainException("Impossible de réserver des tickets pour une performance non programmée.");

            if (quantity <= 0)
                throw new ArgumentException("La quantité doit être positive.", nameof(quantity));

            if (quantity > AvailableTickets)
                throw new InsufficientTicketsException(quantity, AvailableTickets);

            AvailableTickets -= quantity;

            if (AvailableTickets == 0)
                Status = PerformanceStatus.SoldOut;

            UpdateTimestamp();
        }

        public void ReleaseTickets(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("La quantité doit être positive.", nameof(quantity));

            if (AvailableTickets + quantity > Capacity)
                throw new DomainException("Impossible de libérer plus de places que la capacité.");

            AvailableTickets += quantity;

              if (Status == PerformanceStatus.SoldOut)
                Status = PerformanceStatus.Scheduled;

            UpdateTimestamp();
        }

        public void Cancel()
        {
            if (Status == PerformanceStatus.Completed)
                throw new DomainException("Une performance terminée ne peut pas être annulée.");

            Status = PerformanceStatus.Cancelled;
            UpdateTimestamp();
        }

        public void MarkAsCompleted()
        {
            if (Status == PerformanceStatus.Cancelled)
                throw new DomainException("Une performance annulée ne peut pas être marquée comme terminée.");

            Status = PerformanceStatus.Completed;
            UpdateTimestamp();
        }

        // Méthodes de validation privées 
        private static void ValidateSpectacleId(int spectacleId)
        {
            ValidateSpectacleIdPositive(spectacleId);
        }

        private static void ValidateSpectacleIdPositive(int spectacleId)
        {
            if (spectacleId <= 0)
                throw new ArgumentException("L'ID du spectacle doit être strictement positif.", nameof(spectacleId));
        }

        private static void ValidateDate(DateTime date)
        {
            if (date <= DateTime.UtcNow)
                throw new ArgumentException("La date de la performance doit être dans le futur.", nameof(date));
        }

        private static void ValidateCapacity(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("La capacité doit être strictement positive.", nameof(capacity));
        }


        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion
    }
}
