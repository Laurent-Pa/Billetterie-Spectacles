using Billeterie_Spectacles.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Validation
{
    /// <summary>
    /// Valide qu'une chaîne correspond à une valeur valide de SpectacleCategory
    /// </summary>
    public class ValidSpectacleCategoryAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string category)
            {
                if (Enum.TryParse<SpectacleCategory>(category, ignoreCase: true, out _))
                {
                    return ValidationResult.Success;
                }
            }

            string validValues = string.Join(", ", Enum.GetNames<SpectacleCategory>());
            return new ValidationResult($"Catégorie invalide. Valeurs acceptées : {validValues}");
        }
    }
}
