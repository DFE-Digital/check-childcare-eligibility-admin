using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;

namespace CheckChildcareEligibility.Admin.Helpers;

public static class WorkingFamiliesCheckHelper
{

    public static bool ChildIsTooYoung(DateTime dateOfBirth, DateTime validityStartDate)
    {
        return validityStartDate < dateOfBirth.AddMonths(9);
    }

    public static bool ChildIsTooYoung(CheckEligibilityItemWorkingFamilies response, DateTime currentDate)
    {
        return ChildIsTooYoung(GetChildDateOfBirth(response), GetValidityStartDate(response));
    }

    public static bool ChildIsTooOld(DateTime dateOfBirth, DateTime currentDate)
    {
        return HasReachedCompulsorySchoolAge(dateOfBirth, currentDate);
    }

    public static bool ChildIsTooOld(CheckEligibilityItemWorkingFamilies response, DateTime currentDate)
    {
        return HasReachedCompulsorySchoolAge(GetChildDateOfBirth(response), currentDate);
    }

    public static bool IsInGracePeriod(CheckEligibilityItemWorkingFamilies response, DateTime currentDate)
    {
        return currentDate > GetValidityEndDate(response)
            && currentDate < GetGracePeriodEndDate(response);
    }

    public static bool IsExpired(CheckEligibilityItemWorkingFamilies response, DateTime currentDate)
    {
        return currentDate > GetGracePeriodEndDate(response);
    }

    public static bool IsTemporaryCode(CheckEligibilityItemWorkingFamilies response)
    {
        return response.EligibilityCode.StartsWith("1");
    }

    public static bool IsFosterCode(CheckEligibilityItemWorkingFamilies response)
    {
        return response.EligibilityCode.StartsWith("4");
    }

    public static bool IsNotValidYet(CheckEligibilityItemWorkingFamilies response, DateTime currentDate)
    {
        return GetValidityStartDate(response) >= GetTermStart(currentDate);
    }

    public static bool IsReconfirmed(CheckEligibilityItemWorkingFamilies response, DateTime currentDate)
    {
        var gracePeriodEndDate = GetGracePeriodEndDate(response);
        var endTermName = GetTermName(GetTermStart(gracePeriodEndDate));
        return response.Status == "eligible"
            && !IsNotValidYet(response, currentDate)
            && !IsInGracePeriod(response, currentDate)
            && GetTermName(currentDate) != endTermName;
    }

    public static (Term Current, Term Next) GetTerms(DateTime date)
    {
        int year = date.Year;

        if (date >= new DateTime(year, 9, 1))
        {
            return (
                new Term(TermName.Autumn, new DateTime(year, 9, 1)),
                new Term(TermName.Spring, new DateTime(year + 1, 1, 1))
            );
        }

        if (date >= new DateTime(year, 4, 1))
        {
            return (
                new Term(TermName.Summer, new DateTime(year, 4, 1)),
                new Term(TermName.Autumn, new DateTime(year, 9, 1))
            );
        }

        return (
            new Term(TermName.Spring, new DateTime(year, 1, 1)),
            new Term(TermName.Summer, new DateTime(year, 4, 1))
        );
    }

    public static string GetTermInfo(CheckEligibilityItemWorkingFamilies response, DateTime vsd, DateTime currentDate)
    {
        DateTime nineMonthsDate = GetChildDateOfBirth(response).AddMonths(9);

        if (ChildIsTooYoung(response, currentDate) && vsd < nineMonthsDate)
        {
            vsd = nineMonthsDate;
        }

        if (currentDate > GetValidityEndDate(response))
        {
            return $"{GetGracePeriodEndDate(response):dd MMMM yyyy}";
        }

        if (vsd < GetTermStart(currentDate))
        {
            return GetTermName(currentDate);
        }

        DateTime springStart = new DateTime(vsd.Year, 1, 1);
        DateTime summerStart = new DateTime(vsd.Year, 4, 1);
        DateTime autumnStart = new DateTime(vsd.Year, 9, 1);

        if (vsd >= autumnStart) { return $"{WorkingFamiliesResponseBanner.SpringTerm} {vsd.AddYears(1).Year}"; }

        if (vsd >= summerStart) { return $"{WorkingFamiliesResponseBanner.AutumnTerm} {vsd.Year}"; }

        return $"{WorkingFamiliesResponseBanner.SummerTerm} {vsd.Year}";
    }

    public static string GetTermName(DateTime date)
    {
        DateTime springStart = new DateTime(date.Year, 1, 1);
        DateTime summerStart = new DateTime(date.Year, 4, 1);
        DateTime autumnStart = new DateTime(date.Year, 9, 1);
        if (date >= autumnStart) { return $"{WorkingFamiliesResponseBanner.AutumnTerm} {date.Year}"; }
        if (date >= summerStart) { return $"{WorkingFamiliesResponseBanner.SummerTerm} {date.Year}"; }
        return $"{WorkingFamiliesResponseBanner.SpringTerm} {date.Year}";
    }

    public static DateTime GetTermStart(DateTime date)
    {
        int year = date.Year;
        if (date >= new DateTime(year, 9, 1)) { return new DateTime(year, 9, 1); }
        if (date >= new DateTime(year, 4, 1)) { return new DateTime(year, 4, 1); }
        return new DateTime(year, 1, 1);
    }

    public static DateTime GetNextTerm(DateTime termStart)
    {
        if (termStart.Month == 1) { return new DateTime(termStart.Year, 4, 1); }
        if (termStart.Month == 4) { return new DateTime(termStart.Year, 9, 1); }
        return new DateTime(termStart.Year + 1, 1, 1);
    }

    public static bool HasReachedCompulsorySchoolAge(DateTime dateOfBirth, DateTime currentCheckDate)
    {
        DateTime fifthBirthday = dateOfBirth.AddYears(5);
        DateTime termChildTurnsFive = GetTermStart(fifthBirthday);
        DateTime termAfterFive = GetNextTerm(termChildTurnsFive);
        return currentCheckDate >= termAfterFive;
    }

    private static DateTime GetValidityStartDate(CheckEligibilityItemWorkingFamilies response)
    {
        return DateTime.Parse(response.ValidityStartDate);
    }

    private static DateTime GetValidityEndDate(CheckEligibilityItemWorkingFamilies response)
    {
        return DateTime.Parse(response.ValidityEndDate);
    }

    private static DateTime GetGracePeriodEndDate(CheckEligibilityItemWorkingFamilies response)
    {
        return DateTime.Parse(response.GracePeriodEndDate);
    }

    private static DateTime GetChildDateOfBirth(CheckEligibilityItemWorkingFamilies response)
    {
        return DateTime.Parse(response.DateOfBirth);
    }

}
