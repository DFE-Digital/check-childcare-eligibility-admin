using System.ComponentModel.DataAnnotations;
using System.Reflection;
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
        var modelType = model.GetType();
        var property = modelType.GetProperty(validationContext.DisplayName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null) { return new ValidationResult($"Model does not contain a {validationContext.DisplayName} property"); }

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

        var nino = NinoValidation.Normalize(value.ToString())!;

        if (nino.Length > 9)
        {
            return new ValidationResult(
                "National Insurance number should contain no more than 9 alphanumeric characters");
        }

        if (!NinoValidation.IsValidCanonical(nino))
        {
            return new ValidationResult("Enter a National Insurance number in the correct format");
        }
        // Set the cleaned NINO back into the model
        property.SetValue(model, nino);

        return ValidationResult.Success;
    }
}