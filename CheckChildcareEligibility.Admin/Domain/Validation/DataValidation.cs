using System.Text.RegularExpressions;
using CheckChildcareEligibility.Admin.Attributes;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Helpers;

namespace CheckYourEligibility.API.Domain.Validation;

internal static class DataValidation
{
    internal static bool BeAValidName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, NameAttribute.NameValidationRegex);
    }

    internal static bool BeAValidDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string[] formats =
        [
            "yyyy-MM-dd",
            "dd-MM-yyyy"
        ];

        return DateTime.TryParseExact(
            value,
            formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out _);
    }

    internal static bool BeAPastDate(DateTime value)
    {
        return value.Date <= DateTime.Today;
    }

    internal static bool BeWithin31Days(DateTime value)
    {
        DateTime backdateWindow = DateTime.Today.AddDays(-31);
        return value >= backdateWindow;
    }

    internal static bool BeAValidNi(string? value)
    {
        return NinoValidation.IsValidInput(NinoValidation.RemoveSpaces(value));
    }

    internal static bool BeAValidUkPostcode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        const string regexString =
            @"^(GIR\s?0AA|(?:[A-PR-UWYZ][0-9][0-9]?|[A-PR-UWYZ][A-HK-Y][0-9][0-9]?|[A-PR-UWYZ][0-9][A-HJKPSTUW]|[A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY])\s?[0-9][ABD-HJLNP-UW-Z]{2})$";

        return Regex.IsMatch(
            value.Trim().ToUpperInvariant(),
            regexString,
            RegexOptions.Compiled);
    }

    internal static bool BeAValidChildAge(DateTime value)
    {
        DateTime fifthBirthday = value.AddYears(5);
        var (_, termAfterBirthday) = WorkingFamiliesCheckHelper.GetTerms(fifthBirthday);
        return DateTime.Today < termAfterBirthday.StartDate;
    }
}