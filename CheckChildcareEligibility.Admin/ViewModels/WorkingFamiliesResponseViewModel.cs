using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class WorkingFamiliesResponseViewModel
    {
        public CheckEligibilityItemWorkingFamilies Response { get; set; }
        public bool ChildIsTooYoung => Response.ValidityStartDate < ChildDateOfBirth.AddMonths(9);   
        public bool IsEligible => Response.Status == CheckEligibilityStatus.eligible.ToString();
        public bool IsInGracePeriod => DateTime.UtcNow > Response.ValidityEndDate && DateTime.UtcNow < Response.GracePeriodEndDate;
  
        public string GracePeriodEndDisplay =>
            (IsEligible && ChildIsTooYoung) || IsNotValidYet
                ? WorkingFamiliesResponseDetails.GracePeriodEndDateNotAvailable
                : Response.GracePeriodEndDate.ToString("d MMMM yyyy");
        public string ReconfirmationDateLabel =>
            Response.EligibilityCodeType == EligibilityCodeType.Temporary
                ? "Apply for a new code by"
                : "Reconfirm between";

        public string ReconfrimBetween
        {
            get
            {
                if (Response.EligibilityCodeType == EligibilityCodeType.Temporary)
                {
                    return Response.ValidityEndDate.ToString("d MMMM yyyy");
                }

                if (Response.ReconfirmationProperties?.Status == ReconfirmationStatus.ChildTooOld)
                {
                    return WorkingFamiliesResponseDetails.StatusNotApplicable;
                }

                return $"{Response.ReconfirmationProperties?.StartDate:d MMMM yyyy} and {Response.ReconfirmationProperties?.EndDate:d MMMM yyyy}";
            }
        }

        public DateTime ChildDateOfBirth => DateTime.Parse(Response.DateOfBirth);    
        public string CodeType = WorkingFamiliesResponseBanner.CodePermanent;
        public string CodeStatus = WorkingFamiliesResponseBanner.CodeValid;
        public string BannerColour = WorkingFamiliesResponseBanner.ColourGreen;
        public string TermValidityDetails = WorkingFamiliesResponseBanner.TermValidFor;
        public string TermValidityDateRange = string.Empty;
        public TermName? CurrentTerm => Response.TermValidity.Current;
        public TermName? NextTerm => Response.TermValidity.Next;

        public bool IsNotValidYet
        {
            get
            {
                // If current term is None and next term is assigned
                return CurrentTerm == TermName.None && NextTerm != TermName.None;
            }
        }
        public bool IsReconfirmed
        {
            get
            {
                return IsEligible && !IsNotValidYet && !IsInGracePeriod && NextTerm != TermName.None;
            }
        }

        public string SetBannerCodeType()
        {

            if (Response.ReconfirmationProperties?.Status == ReconfirmationStatus.ChildTooOld)
            {
                return WorkingFamiliesResponseBanner.ReconfirmationChildTooOld;
            }
            else if (Response.ReconfirmationProperties?.Status == ReconfirmationStatus.Due)
            {
                return $"{WorkingFamiliesResponseBanner.ReconfirmationBefore} {Response.ValidityEndDate.ToString("d MMMM yyyy")}";
            }
            else if (Response.ReconfirmationProperties?.Status == ReconfirmationStatus.Overdue) 
            {
                return WorkingFamiliesResponseBanner.ReconfirmationOverdue;
            }
            return string.Empty;
        }

        public void SetBannerValues()
        {
            if (Response.EligibilityCodeType == EligibilityCodeType.Temporary)
            {
                CodeType = WorkingFamiliesResponseBanner.CodeTemporary;

                if (IsEligible)
                {
                    TermValidityDetails = "Only "  + TermValidityDetails;
                }
            }
            else if (Response.EligibilityCodeType == EligibilityCodeType.Foster)
            {
                CodeType = WorkingFamiliesResponseBanner.CodeFosterFamily;
            }

            if (IsEligible && ChildIsTooYoung) // Child too young
            {

                DateTime nineMonthsDate = ChildDateOfBirth.AddMonths(9);
                CodeStatus = WorkingFamiliesResponseBanner.CodeChildTooYoung;
                BannerColour = WorkingFamiliesResponseBanner.ColourBlue;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFrom} {NextTerm} {nineMonthsDate.Year}";

            }
            else if (IsNotValidYet) // Code cannot be used yet
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeNotValidYet;
                BannerColour = WorkingFamiliesResponseBanner.ColourBlue;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFrom} {NextTerm.ToString()} {Response.GracePeriodEndDate.Year}";

            }
            // is Valid and reconfirmation has happened
            else if (IsReconfirmed)
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeValid;
                BannerColour = WorkingFamiliesResponseBanner.ColourGreen;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFor} {CurrentTerm} {DateTime.UtcNow.Year} and {NextTerm} {Response.GracePeriodEndDate.Year}";
            }

            else if (Response.Status == CheckEligibilityStatus.notEligible.ToString()) // Expired
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeExpired;
                BannerColour = WorkingFamiliesResponseBanner.ColourOrange;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermExpiredOn} {Response.GracePeriodEndDate:dd MMMM yyyy}";
            }
            else if (IsInGracePeriod)
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeInGracePeriod;
                BannerColour = WorkingFamiliesResponseBanner.ColourYellow;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermExpiresOn} {Response.GracePeriodEndDate:dd MMMM yyyy}";
            }
           
        }

        public string SetBannerReconfirmationMessage()
        {
             if (Response.ReconfirmationProperties?.Status == ReconfirmationStatus.Due)
            {
                return $"{WorkingFamiliesResponseBanner.ReconfirmationBefore} {Response.ReconfirmationProperties.EndDate?.ToString("d MMMM yyyy")}";
            }
            else if (Response.ReconfirmationProperties?.Status == ReconfirmationStatus.Overdue)
            {
                return WorkingFamiliesResponseBanner.ReconfirmationOverdue;
            }
            else if (Response.ReconfirmationProperties?.Status == ReconfirmationStatus.ChildTooOld)
            {
                return WorkingFamiliesResponseBanner.ReconfirmationChildTooOld;
            }
            return string.Empty;
        }

        public string[] SetReconfirmationStatus()
        {
            if (Response.ReconfirmationProperties.Status == ReconfirmationStatus.Due)
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusDueNow;
            }
            if (Response.ReconfirmationProperties.Status == ReconfirmationStatus.NotDueYet)
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusNotDueYet;
            }
            if (Response.ReconfirmationProperties.Status == ReconfirmationStatus.Overdue)
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusOverdue;
            }
            if (Response.ReconfirmationProperties.Status == ReconfirmationStatus.NotApplicable)
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusNotApplicable;
            }
             if (Response.ReconfirmationProperties.Status == ReconfirmationStatus.ChildTooOld)
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusChildTooOld;
            }                           
            return Array.Empty<string>();
        }

    }
}