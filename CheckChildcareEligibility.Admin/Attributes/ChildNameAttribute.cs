using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.Attributes;

public class ChildNameAttribute : ValidationAttribute
{
    private readonly string _fieldName;

    public ChildNameAttribute(string fieldName)
    {
        _fieldName = fieldName;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var child = validationContext.ObjectInstance as Child;

        if (child == null) return new ValidationResult("Invalid child instance.");

        var childIndex = child.ChildIndex;

        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult($"Enter a {_fieldName} for child {childIndex}");
        return ValidationResult.Success;
    }
}