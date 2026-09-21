using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CheckChildcareEligibility.Admin.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NinoAttribute : ValidationAttribute
{
    const string validNinoRegex = @"^(?!BG)(?!GB)(?!NK)(?!KN)(?!TN)(?!NT)(?!ZZ)(?:[A-CEGHJ-PR-TW-Z][A-CEGHJ-NPR-TW-Z])(?:\s*\d\s*){6}([A-D]|\s)$";

    private readonly Regex _regex;

    public NinoAttribute()
    {
        ErrorMessage = "Enter a National Insurance number in the correct format";
        _regex = new Regex(validNinoRegex, RegexOptions.Compiled);
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
                return new ValidationResult("National Insurance number is required");
            }
        }

        var nino = new string(value.ToString()
                               .ToUpperInvariant()
                               .Where(char.IsLetterOrDigit)
                               .ToArray());

        if (nino.Length > 9)
        {
            return new ValidationResult(
                "National Insurance number should contain no more than 9 alphanumeric characters");
        }

        if (!_regex.IsMatch(nino))
        {
            return new ValidationResult("Enter a National Insurance number in the correct format");
        }
        // Set the cleaned NINO back into the model
        property.SetValue(model, nino);

        return ValidationResult.Success;
    }
}