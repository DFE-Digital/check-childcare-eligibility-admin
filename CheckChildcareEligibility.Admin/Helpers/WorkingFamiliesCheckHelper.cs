using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.Helpers;

public static class WorkingFamiliesCheckHelper
{


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

    public static string GetReconfirmBetween(EligibilityCodeType? codeType, ReconfirmationProperties? properties, DateTime? validityEndDate = null)
    {
        if (codeType == EligibilityCodeType.Temporary)
        {
            return validityEndDate?.ToString("d MMMM yyyy");
        }
        if (properties?.Status == ReconfirmationStatus.ChildTooOld)
        {
            return WorkingFamiliesResponseDetails.StatusNotApplicable;
        }
        return $"{properties?.StartDate:d MMMM yyyy} and {properties?.EndDate:d MMMM yyyy}";
    }

    public static string[] GetReconfirmStatus(ReconfirmationProperties properties)
    {
        if (properties.Status == ReconfirmationStatus.Due)
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusDueNow;
        }
        if (properties.Status == ReconfirmationStatus.NotDueYet)
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusNotDueYet;
        }
        if (properties.Status == ReconfirmationStatus.Overdue)
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusOverdue;
        }
        if (properties.Status == ReconfirmationStatus.NotApplicable)
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusNotApplicable;
        }
        if (properties.Status == ReconfirmationStatus.ChildTooOld)
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusChildTooOld;
        }
        return Array.Empty<string>();
    }

    public static string GetCodeStatus_FF(EligibilityCodeProperties properties)
    {
        var nextTermView = WorkingFamiliesResponseBanner.TermNamesInView.GetValueOrDefault(properties.NextTerm.Name, string.Empty);
        var currentTermView = WorkingFamiliesResponseBanner.TermNamesInView.GetValueOrDefault(properties.CurrentTerm.Name, string.Empty);

        bool isExpired = properties.GracePeriodEndDate < DateTime.UtcNow.Date;
        string result = "<unknown>";

        if (properties.ChildIsTooYoung) // Child too young
        {
            result = WorkingFamiliesResponseBanner.CodeChildTooYoung;
        }
        else if (properties.ChildIsTooOld)
        {
            result = WorkingFamiliesResponseBanner.CodeChildTooOld;
        }
        else if (isExpired)
        {
            result = $"Code {WorkingFamiliesResponseBanner.CodeExpired}";
        }
        else if (properties.IsNotValidYet) // Code cannot be used yet
        {
            result = $"Code {WorkingFamiliesResponseBanner.CodeNotValidYet}";
        }
        // Is valid and reconfirmation has happened
        else if (properties.IsReconfirmed)
        {
            result = $"{WorkingFamiliesResponseBanner.TermValidFor} {currentTermView} {DateTime.UtcNow.Year} and {nextTermView} {properties.GracePeriodEndDate.Year}";
        }
        else if (properties.IsInGracePeriod)
        {
            result = $"{WorkingFamiliesResponseBanner.TermExpiresOn} {properties.GracePeriodEndDate:dd MMMM yyyy}";
        }
        else
        {
            result = $"{WorkingFamiliesResponseBanner.TermValidFor} {currentTermView} {DateTime.UtcNow.Year}";
        }
        return result;
    }

}
