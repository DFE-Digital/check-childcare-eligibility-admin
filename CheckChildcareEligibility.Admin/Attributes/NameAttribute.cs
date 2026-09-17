using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckChildcareEligibility.Admin.Attributes;

public class NameAttribute : ValidationAttribute
{
    public static readonly string NameValidationRegex = @"^[a-zA-Z" +
            @"ÁáÉéÍíÓóÚúÝýĆćĹĺŃńŔŕŚśŹź" +
            @"ÀàÈèÌìÒòÙùẀẁỲỳ" +
            @"ÂâÊêÎîÔôÛûĈĉĜĝĤĥĴĵŜŝŴŵŶŷ" +
            @"ÃãÑñÕõĨĩŨũẼẽỸỹ" +
            @"ÄäËëÏïÖöÜüŸÿ" +
            @"ÇçĢģĶķĻļŅņŖŗŞşŢţ" +
            @"ÅåŮů" +
            @"ĀāĒēĪīŌōŪūȲȳ" +
            @"ĂăĔĕĞğĬĭŎŏŬŭ" +
            @"ĊċĖėĠġİẊẋŻż" +
            @"ĄąĘęĮįŲų" +
            @"ŐőŰű" +
            @" ,.''\u2018\u2019-]+$";

    private static readonly Regex regex = new(NameValidationRegex);

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        var firstNameProperty = model.GetType()
                            .GetProperties()
                            .FirstOrDefault(p => p.Name.Contains("FirstName"));
        var lastNameProperty = model.GetType()
                            .GetProperties()
                            .FirstOrDefault(p => p.Name.Contains("LastName"));

        var firstName = firstNameProperty?.GetValue(model)?.ToString() ?? string.Empty;
        var lastName = lastNameProperty?.GetValue(model)?.ToString() ?? string.Empty;

        if (firstName == value)
        {
            if (value == null || value == "")
                return ValidationResult.Success;

            if (!regex.IsMatch(value.ToString()))
            {
                var constantName = $"{firstNameProperty.Name}Invalid";
                var field = typeof(FosterFamilyValidationMessages).GetField(constantName);
                if (field != null)
                {
                    var message = field?.GetValue(null)?.ToString();
                    return new ValidationResult(message);
                }
                return new ValidationResult("First Name field contains an invalid character");
            }
        }

        if (lastName == value)
        {
            if (value == null || value == "")
                return ValidationResult.Success;

            if (!regex.IsMatch(value.ToString()))
            {
                var constantName = $"{lastNameProperty.Name}Invalid";
                var field = typeof(FosterFamilyValidationMessages).GetField(constantName);
                if (field != null)
                {
                    var message = field?.GetValue(null)?.ToString();
                    return new ValidationResult(message);
                }
                return new ValidationResult("Last Name field contains an invalid character");
            }
        }

        return ValidationResult.Success;
    }
}
