using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CheckChildcareEligibility.Admin.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NinValidatorAttribute : ValidationAttribute
{
    private readonly Regex _regex;

    public NinValidatorAttribute()
    {
        ErrorMessage = "Enter a National Insurance number in the correct format";
        _regex = new Regex("^[A-Z0-9]{2}\\d{6}[A-D]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        if (value == null)
        {
            if (model is ViewModels.FosterCarerDetailsViewModel ||
                model is ViewModels.FosterPartnerDetailsViewModel)
            {
                return ValidationResult.Success; //Use the ViewModel Required message
            }
            else
            {
                return new ValidationResult("National Insurance number is required");
            }
        }
        else
        {
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
        }

        return ValidationResult.Success;
    }
}