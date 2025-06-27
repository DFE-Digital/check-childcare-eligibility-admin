using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.Attributes;

public class LastNameAttribute : ValidationAttribute
{
    private static readonly string UnicodeOnlyPattern = NameAttribute.UnicodeOnlyPattern;

    private static readonly Regex regex = new(UnicodeOnlyPattern);
    private readonly string _childIndexPropertyName;
    private readonly string _fieldName;
    private readonly string _objectName;

    public LastNameAttribute(string fieldName, string objectName, string? childIndexPropertyName,
        string? errorMessage = null) : base(errorMessage)
    {
        _fieldName = fieldName;
        _objectName = objectName;
        _childIndexPropertyName = childIndexPropertyName;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        if (string.IsNullOrEmpty(value?.ToString())) return ValidationResult.Success;

        if (!regex.IsMatch(value.ToString()))
        {
            return new ValidationResult($"{_fieldName} contains an invalid character");
        }

        return ValidationResult.Success;
    }
}