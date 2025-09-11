using System.ComponentModel.DataAnnotations;

namespace CheckChildcareEligibility.Admin.Attributes;

public class EligibilityCodeAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        string? code = value?.ToString();

        if (string.IsNullOrEmpty(code))
        {
            return new ValidationResult("Enter an eligibility code that is 11 digits long");
        }

        if (!long.TryParse(code, out _))
        {
            return new ValidationResult("Eligibility code must only contain numbers");
        }

        if (code.Length != 11)
        {
            return new ValidationResult("Eligibility code must be 11 digits long");
        }

        return ValidationResult.Success!;
    }
}
