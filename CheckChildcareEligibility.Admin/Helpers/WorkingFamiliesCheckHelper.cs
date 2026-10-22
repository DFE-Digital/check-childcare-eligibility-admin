using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants;
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

    public static string GetReconfirmBetween(EligibilityCodeProperties properties)
    {
        if (properties.Type == EligibilityCodeType.Temporary)
        {
            return properties.ValidityEndDate.ToString("d MMMM yyyy");
        }
        if (properties.ChildTooOld)
        {
            return WorkingFamiliesResponseDetails.StatusNotApplicable;
        }
        return $"{properties.ReconfirmationProperties?.StartDate:d MMMM yyyy} and {properties.ReconfirmationProperties?.EndDate:d MMMM yyyy}";
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
        if (properties == null) { return string.Empty; }

        var nextTermView = WorkingFamiliesResponseBanner.TermNamesInView.GetValueOrDefault(properties.NextTerm.Name, string.Empty);
        var currentTermView = WorkingFamiliesResponseBanner.TermNamesInView.GetValueOrDefault(properties.CurrentTerm.Name, string.Empty);

        string result = "<unknown>";
        if (properties.ChildTooOld || properties.IsExpired)
        {
            result = FosterFamiliesConstants.CodeExpired;
        }
        else if (properties.ChildTooYoung)
        {
            result = FosterFamiliesConstants.CodeChildTooYoung;
        }
        else if (properties.IsNotValidForCurrentTerm || properties.IsNeverValid)
        {
            result = FosterFamiliesConstants.CodeNotValidYet;
        }
        else if (properties.IsInGracePeriod)
        {
            result = FosterFamiliesConstants.CodeInGracePeriod;
        }
        else
        {
            result = $"{FosterFamiliesConstants.CodeValidFor} {currentTermView} {properties.CurrentTerm.StartDate.Year}";
            if (nextTermView != string.Empty)
            {
                result += $" and {nextTermView} {properties.NextTerm.StartDate.Year}";
            }
        }
        return result;
    }
}
