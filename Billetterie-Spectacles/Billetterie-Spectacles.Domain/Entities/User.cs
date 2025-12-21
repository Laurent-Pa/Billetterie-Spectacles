using Billetterie_Spectacles.Domain.Enums;
using System.Text.RegularExpressions;

namespace Billetterie_Spectacles.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; private set; }
        public UserRole Role { get; private set; } = UserRole.Client;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        // Un utilisateur (Client) peut passer plusieurs commandes
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        // Un utilisateur (Organizer) peut créer plusieurs spectacles
        public ICollection<Spectacle> CreatedSpectacles { get; set; } = new List<Spectacle>();

        #region Constructors
        private User() { }

        public User(
            string name, 
            string surname, 
            string email, 
            string password, 
            UserRole role = UserRole.Client, 
            string? phone = null)
        {
            SetName(name);
            SetSurname(surname);
            SetEmail(email);
            SetPassword(password);
            SetPhone(phone);
            ValidateRole(role);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion


        #region Methods
        // Méthodes métier avec validations
        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Le nom ne peut pas être vide.", nameof(name));

            if (name.Length > 100)
                throw new ArgumentException("Le nom ne peut pas dépasser 100 caractères.", nameof(name));

            Name = name.Trim();
            UpdateTimestamp();
        }

        public void SetSurname(string surname)
        {
            if (string.IsNullOrWhiteSpace(surname))
                throw new ArgumentException("Le prénom ne peut pas être vide.", nameof(surname));

            if (surname.Length > 100)
                throw new ArgumentException("Le prénom ne peut pas dépasser 100 caractères.", nameof(surname));

            Surname = surname.Trim();
            UpdateTimestamp();
        }

        public void SetEmail(string email)
        {
            ValidateEmailNotEmpty(email);
            ValidateEmailFormat(email);
            Email = email.ToLowerInvariant().Trim();
            UpdateTimestamp();
        }
        private static void ValidateEmailNotEmpty(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("L'email ne peut pas être vide.", nameof(email));
        }

        private static void ValidateEmailFormat(string email)
        {
            // Regex simple pour validation email
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(email))
                throw new ArgumentException("Le format de l'email est invalide.", nameof(email));
        }

        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Le mot de passe ne peut pas être vide.", nameof(password));

            if (password.Length < 8)
                throw new ArgumentException("Le mot de passe doit contenir au moins 8 caractères.", nameof(password));

            // TODO: Hasher le mot de passe (bcrypt, Argon2) avant de le stocker
            Password = password;
            UpdateTimestamp();
        }

        public void SetPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                Phone = null;
            }
            else
            {
                ValidatePhoneFormat(phone);
                // Nettoyer le numéro (enlever espaces, tirets, parenthèses)
                Phone = Regex.Replace(phone, @"[\s\-\(\)\.]", "");
            }
            UpdateTimestamp();
        }

        private static void ValidatePhoneFormat(string phone)
        {
            // Nettoyer le numéro pour validation
            var cleaned = Regex.Replace(phone, @"[\s\-\(\)\.]", "");

            // Accepte les formats français : 0612345678 ou +33612345678
            var regex = new Regex(@"^(\+33|0)[1-9]\d{8}$");
            if (!regex.IsMatch(cleaned))
                throw new ArgumentException("Le format du numéro de téléphone est invalide.", nameof(phone));
        }

        private static void ValidateRole(UserRole role)
        {
            if (!Enum.IsDefined(typeof(UserRole), role))
                throw new ArgumentException("Le rôle spécifié n'est pas valide.", nameof(role));
        }

        public void ChangeRole(UserRole newRole)
        {
            ValidateRole(newRole);  // ← Ajout de la validation
            Role = newRole;
            UpdateTimestamp();
        }

        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion
    }
}
