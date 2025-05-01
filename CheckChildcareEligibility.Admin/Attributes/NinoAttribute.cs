using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CheckChildcareEligibility.Admin.Models;
using static CheckChildcareEligibility.Admin.Models.ParentGuardian;

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
        var model = (ParentGuardian)validationContext.ObjectInstance;

        //Nin not provided
        if (value == null)
            return new ValidationResult("Enter a National Insurance number");

        //Nin selected and completed - validate against regex
        if (value != null)
        {
            var nino = value.ToString().ToUpper();
            nino = string.Concat(nino
                .Where(ch => char.IsLetterOrDigit(ch)));

            if (nino.Length > 9)
                return new ValidationResult(
                    "National Insurance number should contain no more than 9 alphanumeric characters");

            if (!regex.IsMatch(nino)) return new ValidationResult("Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");

            model.NationalInsuranceNumber = nino;
        }

        return ValidationResult.Success;
    }
}