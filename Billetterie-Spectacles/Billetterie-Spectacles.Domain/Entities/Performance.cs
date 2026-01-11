using Billetterie_Spectacles.Domain.Enums;
using Billetterie_Spectacles.Domain.Exceptions;

namespace Billetterie_Spectacles.Domain.Entities
{
    public class Performance
    {
        public int PerformanceId { get; set; }
        public DateTime Date { get; set; }
        public PerformanceStatus Status { get; private set; } = PerformanceStatus.Scheduled;
        public decimal UnitPrice { get; private set; }
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

        public Performance(int spectacleId, DateTime date, int capacity, decimal unitPrice)
        {
            ValidateSpectacleId(spectacleId);
            ValidateDate(date);
            ValidateCapacity(capacity);
            ValidateUnitPrice(unitPrice);

            SpectacleId = spectacleId;
            Date = date;
            UnitPrice = unitPrice;
            Capacity = capacity;
            AvailableTickets = capacity; // A l'initialisation il n'y a pas de tickets vendus
            Status = PerformanceStatus.Scheduled;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion

        #region Methods
        // Methodes métier

        /// <summary>
        /// Réserve des tickets (diminue AvailableTickets)
        /// Gère automatiquement le passage à SoldOut si plus de places
        /// </summary>
        public void BookTickets(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("La quantité doit être positive.", nameof(quantity));

            if (Status == PerformanceStatus.Cancelled)
                throw new DomainException("Impossible de réserver des billets pour une performance annulée.");

            if (Status == PerformanceStatus.Completed)
                throw new DomainException("Impossible de réserver des billets pour une performance terminée.");

            if (AvailableTickets < quantity)
                throw new DomainException($"Pas assez de places disponibles. Disponibles : {AvailableTickets}");

            AvailableTickets -= quantity;

            // ✅ Passage automatique à SoldOut
            if (AvailableTickets == 0)
            {
                Status = PerformanceStatus.SoldOut;
            }

            UpdateTimestamp();
        }

        /// <summary>
        /// Libère des tickets (augmente AvailableTickets)
        /// Gère automatiquement le retour à Scheduled si c'était SoldOut
        /// </summary>
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

                // <summary>
        /// Annule la performance (change le statut à Cancelled)
        /// </summary>
        public void Cancel()
        {
            if (Status == PerformanceStatus.Cancelled)
                throw new DomainException("La performance est déjà annulée.");

            if (Status == PerformanceStatus.Completed)
                throw new DomainException("Impossible d'annuler une performance terminée.");

            Status = PerformanceStatus.Cancelled;
            UpdateTimestamp();
        }

        /// <summary>
        /// Marque la performance comme terminée
        /// Nécessite un job/worker en arrière plan exécuté tous les jours
        /// </summary>
        public void MarkAsCompleted()
        {
            if (Date > DateTime.UtcNow)
                throw new DomainException("La performance n'a pas encore eu lieu.");

            if (Status == PerformanceStatus.Cancelled)
                throw new DomainException("Impossible de marquer comme terminée une performance annulée.");

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

        private static void ValidateUnitPrice(decimal unitPrice)
        {
            if (unitPrice < 0)
                throw new ArgumentException("Le prix unitaire doit être strictement positif.", nameof(unitPrice));
        }

        /// <summary>
        /// Met à jour les détails de la performance
        /// </summary>
        public void UpdateDetails(DateTime date, int capacity, decimal unitPrice)
        {
            // Validation : Date future
            if (date <= DateTime.UtcNow)
                throw new ArgumentException("La date doit être dans le futur.", nameof(date));

            // Validation : Capacité positive
            if (capacity <= 0)
                throw new ArgumentException("La capacité doit être positive.", nameof(capacity));

            // Validation : Prix positif ou nul
            if (unitPrice < 0)
                throw new ArgumentException("Le prix ne peut pas être négatif.", nameof(unitPrice));

            // Validation CRITIQUE : Vérifier la capacité vs billets vendus
            int ticketsSold = Capacity - AvailableTickets;  // Calculer les billets déjà vendus

            if (capacity < ticketsSold)
            {
                throw new DomainException(
                    $"Impossible de réduire la capacité à {capacity}. " +
                    $"{ticketsSold} billets ont déjà été vendus."
                );
            }

            // Mettre à jour les propriétés
            Date = date;
            Capacity = capacity;
            UnitPrice = unitPrice;

            // Recalculer AvailableTickets
            AvailableTickets = capacity - ticketsSold;

            if (AvailableTickets == 0 && Status != PerformanceStatus.SoldOut)
            {
                Status = PerformanceStatus.SoldOut;     // Sold out si l'organisateur retire de la vente les dernières places dispo
            }
            else if (AvailableTickets > 0 && Status == PerformanceStatus.SoldOut)
            {
                Status = PerformanceStatus.Scheduled;  // Retour à Scheduled si places libérées
            }

            // Mettre à jour le timestamp pour le champs UpdatedAt
            UpdateTimestamp();
        }


        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion
    }
}
