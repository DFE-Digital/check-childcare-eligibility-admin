using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckChildcareEligibility.Admin.Attributes;

public class NameAttribute : ValidationAttribute
{
    private static readonly string UnicodeOnlyPattern = @"^[\p{L}\-']+$";

    private static readonly Regex regex = new(UnicodeOnlyPattern);

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        var lastName = model.GetType().GetProperty("LastName").GetValue(model);

        if (lastName == value)
        {
            if (value == null || value == "")
                return ValidationResult.Success;

            if (!regex.IsMatch(value.ToString()))
                return new ValidationResult("Enter a last name with valid characters");
        }

        return ValidationResult.Success;
    }
}