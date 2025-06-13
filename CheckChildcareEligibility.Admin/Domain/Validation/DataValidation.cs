using System.Text.RegularExpressions;

namespace CheckYourEligibility.API.Domain.Validation;

internal static class DataValidation
{
    internal static bool BeAValidName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var regexString =
            @"^[^\d]*$";
        var rg = new Regex(regexString);
        var res = rg.Match(value);
        return res.Success;
    }

    internal static bool BeAValidDate(string value)
    {
        //parse value here rather than receive pre-parsed value so system can recognise malformed dates such as '123/01/90' or 'November', else they report as empty field.
        value = DateTime.TryParse(value, out var dtval) ? dtval.ToString("yyyy-MM-dd") : string.Empty;

        var regexString =
            @"^\d{4}-\d{2}-\d{2}$";
        var rg = new Regex(regexString);
        var res = rg.Match(value);
        return res.Success;
    }

    internal static bool BeAValidNi(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var regexString =
            @"^(?!BG)(?!GB)(?!NK)(?!KN)(?!TN)(?!NT)(?!ZZ)(?:[A-CEGHJ-PR-TW-Z][A-CEGHJ-NPR-TW-Z])(?:\s*\d\s*){6}([A-D]|\s)$";
        var rg = new Regex(regexString);
        var res = rg.Match(value);
        return res.Success;
    }
}