using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;

namespace CheckChildcareEligibility.Admin.Helpers;

public static class WorkingFamiliesCheckHelper
{

    public static bool ChildIsTooYoung(DateTime dateOfBirth, DateTime validityStartDate)
    {
        return validityStartDate < dateOfBirth.AddMonths(9);
    }

    public static bool ChildIsTooOld(DateTime dateOfBirth, DateTime currentDate)
    {
        return HasReachedCompulsorySchoolAge(dateOfBirth, currentDate);
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
}
