using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Reflection;

namespace CheckChildcareEligibility.Admin.Attributes;

public class NinoAttribute : ValidationAttribute
{
    private static readonly string FirstLetterPattern = "[ABCEGHJKLMNOPRSTWXYZ]";
    private static readonly string SecondLetterPattern = "[ABCEGHJKLMNPRSTWXYZ]";
    private static readonly string DisallowedPrefixesPattern = "^(?!BG|GB|KN|NK|NT|TN|ZZ)";
    private static readonly string NumericPattern = "[0-9]{6}";
    private static readonly string LastLetterPattern = "[ABCD]";

    private static readonly string Pattern = DisallowedPrefixesPattern + FirstLetterPattern + SecondLetterPattern +
                                             NumericPattern + LastLetterPattern;

    private static readonly Regex regex = new(Pattern);

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;
        var modelType = model.GetType();

        // Try to get the NationalInsuranceNumber property
        var property = modelType.GetProperty("NationalInsuranceNumber", BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
        {
            return new ValidationResult("Model does not contain a NationalInsuranceNumber property");
        }

        // NINO not provided
        if (value == null)
        {
            return new ValidationResult("Enter a National Insurance number");
        }

        // Clean and validate the NINO
        var nino = value.ToString().ToUpper();
        nino = string.Concat(nino.Where(char.IsLetterOrDigit));

        if (nino.Length > 9)
        {
            return new ValidationResult("National Insurance number should contain no more than 9 alphanumeric characters");
        }

        if (!regex.IsMatch(nino))
        {
            return new ValidationResult("Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");
        }

        // Set the cleaned NINO back into the model
        property.SetValue(model, nino);

        return ValidationResult.Success;
    }
}
