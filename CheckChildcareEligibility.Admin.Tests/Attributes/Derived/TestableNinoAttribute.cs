using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.Attributes;

namespace CheckChildcareEligibility.Admin.Tests.Attributes.Derived;

public class TestableNinoAttribute : NinoAttribute
{
    public ValidationResult NinoIsValid(object value, ValidationContext validationContext)
    {
        return IsValid(value, validationContext);
    }
}