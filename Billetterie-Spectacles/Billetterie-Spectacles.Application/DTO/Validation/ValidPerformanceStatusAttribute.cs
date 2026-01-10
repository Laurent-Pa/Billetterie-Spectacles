using Billetterie_Spectacles.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Billetterie_Spectacles.Application.DTO.Validation
{
    /// <summary>
    /// Valide qu'une chaîne correspond à une valeur valide de PerformanceStatus
    /// </summary>
    public class ValidPerformanceStatusAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string status)
            {
                if (Enum.TryParse<PerformanceStatus>(status, ignoreCase: true, out _))
                {
                    return ValidationResult.Success;
                }
            }

            string validValues = string.Join(", ", Enum.GetNames<PerformanceStatus>());
            return new ValidationResult($"Statut invalide. Valeurs acceptées : {validValues}");
        }
    }
}
