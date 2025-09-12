using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Models;

public static class Extensions
{
    public static string GetFsmStatusDescription(this string status)
    {
        Enum.TryParse(status, out CheckEligibilityStatus statusEnum);

        switch (statusEnum)
        {
            case CheckEligibilityStatus.parentNotFound:
                return "Information does not match records";
            case CheckEligibilityStatus.eligible:
                return "Eligible";
            case CheckEligibilityStatus.notEligible:
                return "May not be eligible";
            case CheckEligibilityStatus.error:
                return "Try again";
            default:
                return status;
        }
    }

    public static string GetFsmStatusDescription(this CheckEligibilityStatus status)
    {
        return GetFsmStatusDescription(status.ToString());
    }
}