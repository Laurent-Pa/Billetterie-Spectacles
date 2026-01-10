using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Validation
{
    public class FutureDateAttribute : ValidationAttribute
    {
        public FutureDateAttribute()
        {
            // Message par défaut si non spécifié
            ErrorMessage = "La date doit être dans le futur";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date > DateTime.UtcNow)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(ErrorMessage);
            }
            return new ValidationResult("Date invalide");
        }
    }
}
