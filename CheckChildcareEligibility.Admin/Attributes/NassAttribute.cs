using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.API.Domain.Validation;
using System.ComponentModel.DataAnnotations;

namespace CheckChildcareEligibility.Admin.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NassAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null || value is string { Length: 0 })
        {
            return ValidationResult.Success;
        }

        return value is string nass && DataValidation.BeAValidNass(nass)
            ? ValidationResult.Success
            : new ValidationResult(ValidationMessages.InvalidNASS);
    }
}