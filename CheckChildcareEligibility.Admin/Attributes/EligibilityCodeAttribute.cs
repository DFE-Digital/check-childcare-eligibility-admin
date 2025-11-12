using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using System.ComponentModel.DataAnnotations;

namespace CheckChildcareEligibility.Admin.Attributes;
public class EligibilityCodeAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        string? code = value?.ToString();

        if (string.IsNullOrEmpty(code))
        {
            return new ValidationResult(ValidationMessages.EligibilityCodeNullOrEmpty);
        }

        if (!long.TryParse(code, out _))
        {
            return new ValidationResult(ValidationMessages.EligibilityCodeNumber);
        }

        if (code.Length != 11)
        {
            return new ValidationResult(ValidationMessages.EligibilityCodeIncorrectLength);
        }

        return ValidationResult.Success!;
    }
}