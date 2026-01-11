using Billeterie_Spectacles.Domain.Enums;

namespace Billetterie_Spectacles.Domain.Entities
{
    public class Spectacle
    {
        public int SpectacleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public SpectacleCategory Category { get; private set; }
        public string? Description { get; set; }
        public int Duration { get; set; }       // minutes
        public string? Thumbnail { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Clé étrangère : Créateur du spectacle (Organizer)
        public int CreatedByUserId { get; set; }

        // Propriété de navigation : Spectacle créé par un organisateur
        public User CreatedByUser { get; set; } = null!;

        // Relation : Un spectacle a plusieurs performances (représentations)
        public ICollection<Performance> Performances { get; set; } = new List<Performance>();


        #region Constructors
        private Spectacle() { } // Private Constructor for EntityFramework Core

        public Spectacle(string name, SpectacleCategory category, int duration, int createdByUserId, string? description = null, string? thumbnail = null)
        {
            ValidateName(name);
            ValidateCategory(category);
            ValidateDuration(duration);
            ValidateCreatedByUserId(createdByUserId);

            Name = name.Trim();
            Category = category;
            Duration = duration;
            Description = description?.Trim();
            Thumbnail = thumbnail?.Trim();
            CreatedByUserId = createdByUserId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion

        #region Methods
        // Méthodes de validation
        private static void ValidateName(string name)
        {
            ValidateNameNotEmpty(name);
            ValidateNameLength(name);
        }

        private static void ValidateNameNotEmpty(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Le nom du spectacle ne peut pas être vide.", nameof(name));
        }

        private static void ValidateNameLength(string name)
        {
            if (name.Length > 200)
                throw new ArgumentException("Le nom du spectacle ne peut pas dépasser 200 caractères.", nameof(name));
        }

        private static void ValidateCategory(SpectacleCategory category)
        {
            if (!Enum.IsDefined(category))
                throw new ArgumentException("La catégorie spécifiée n'est pas valide.", nameof(category));
        }

        private static void ValidateDuration(int duration)
        {
            if (duration <= 0)
                throw new ArgumentException("La durée doit être strictement positive (en minutes).", nameof(duration));
        }

        private static void ValidateCreatedByUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("L'ID du créateur doit être positif.", nameof(userId));
        }


        // Méthodes métier
        /// <summary>
        /// Met à jour tous les détails du spectacle
        /// Utilisé pour la modification complète via un formulaire d'édition
        /// </summary>
        public void UpdateDetails(string name, SpectacleCategory category, int duration, string? description = null, string? thumbnail = null)
        {
            ValidateName(name);
            ValidateCategory(category);
            ValidateDuration(duration);

            Name = name.Trim();
            Category = category;
            Duration = duration;
            Description = description?.Trim();
            Thumbnail = thumbnail?.Trim();
            UpdateTimestamp();
        }

        /// <summary>
        /// Modifie uniquement le thumbnail (image) du spectacle
        /// Utilisé pour l'upload/suppression d'image sans modifier les autres détails
        /// </summary>
        /// <param name="thumbnailUrl">URL de la nouvelle image, ou null pour supprimer</param>

        public void SetThumbnail(string? thumbnailUrl)
        {
            // Thumbnail est nullable, donc on accepte null ou une valeur
            Thumbnail = thumbnailUrl?.Trim();
            UpdateTimestamp();
        }

        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion
    }
}
