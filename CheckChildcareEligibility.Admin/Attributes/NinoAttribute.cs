using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.Domain.Validation;

namespace CheckChildcareEligibility.Admin.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NinoAttribute : ValidationAttribute
{
    public NinoAttribute()
    {
        ErrorMessage = "Enter a National Insurance number in the correct format";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        if (value == null)
        {
            if (model is ViewModels.FosterCarerDetailsViewModel || model is ViewModels.FosterPartnerDetailsViewModel)
            {
                return ValidationResult.Success; // Use the ViewModel Required message
            }
            else
            {
                return new ValidationResult("Enter a National Insurance number");
            }
        }

        var nino = value.ToString()!;
        var normalizedNino = NinoValidation.Normalize(nino)!;

        if (normalizedNino.Length > 9)
        {
            return new ValidationResult(
                "National Insurance number should contain no more than 9 alphanumeric characters");
        }

        if (!NinoValidation.IsValidInput(nino))
        {
            return new ValidationResult("Enter a National Insurance number in the correct format");
        }

        return ValidationResult.Success;
    }
}