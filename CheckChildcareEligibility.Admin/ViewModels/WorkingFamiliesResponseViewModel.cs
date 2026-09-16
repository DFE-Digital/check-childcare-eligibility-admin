using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class WorkingFamiliesResponseViewModel
    {

        public CheckEligibilityItemWorkingFamilies Response { get; set; }
        public bool ChildIsTooYoung => Response.ChildTooYoung;
        public bool ChildIsTooOld => Response.ReconfirmationProperties.Status == ReconfirmationStatus.ChildTooOld;
        public bool IsEligible => Response.Status == CheckEligibilityStatus.eligible.ToString();
        public bool IsExpired => Response.GracePeriodEndDate < DateTime.UtcNow.Date;
        public bool IsInGracePeriod => DateTime.UtcNow.Date > Response.ValidityEndDate && DateTime.UtcNow.Date <= Response.GracePeriodEndDate;

        public string GracePeriodEndDisplay =>
            ChildIsTooOld
                ? Response.ValidityEndDate.ToString("d MMMM yyyy")
                : (IsEligible && ChildIsTooYoung) || IsNotValidYet
                    ? WorkingFamiliesResponseDetails.GracePeriodEndDateNotAvailable
                    : Response.GracePeriodEndDate.ToString("d MMMM yyyy");

        public string GracePeriodEndLabel =>
            ChildIsTooOld
                ? "Grace period ended"
                : "Grace period ends";

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
        public string CodeType = string.Empty;
        public string CodeStatus = WorkingFamiliesResponseBanner.CodeValid;
        public string BannerColour = WorkingFamiliesResponseBanner.ColourGreen;
        public string TermValidityDetails = WorkingFamiliesResponseBanner.TermValidFor;
        public string TermValidityDateRange = string.Empty;
        public Term CurrentTerm => Response.TermValidity.Current;
        public Term NextTerm => Response.TermValidity.Next;

        public TermName CurrentTermName => (CurrentTerm?.Name ?? TermName.None);
        public TermName NextTermName => (NextTerm?.Name ?? TermName.None);

        public bool IsNotValidYet
        {
            get
            {
                // If current term is None and next term is assigned
                return CurrentTermName == TermName.None && NextTermName != TermName.None;
            }
        }
        public bool IsReconfirmed
        {
            get
            {
                return IsEligible && !IsNotValidYet && !IsInGracePeriod && NextTermName != TermName.None;
            }
        }


        // If child is too young and the code has not expired do not set code type
        // else apply correct content for code type
        private void SetBannerCodeType()
        {
            if (ChildIsTooYoung && !IsExpired) return;
            if (Response.EligibilityCodeType == EligibilityCodeType.Temporary)
            {
                CodeType = WorkingFamiliesResponseBanner.CodeTemporary;

                if (IsEligible)
                {
                    TermValidityDetails = "Only " + TermValidityDetails;
                }
            }
            else if (Response.EligibilityCodeType == EligibilityCodeType.Foster)
            {
                CodeType = WorkingFamiliesResponseBanner.CodeFosterFamily;
            }
            else
            {
                CodeType = WorkingFamiliesResponseBanner.CodePermanent;
            }
        }

        public void SetBannerValues()
        {
            var nextTermView = WorkingFamiliesResponseBanner.TermNamesInView.GetValueOrDefault(NextTerm.Name, string.Empty);
            var currentTermView = WorkingFamiliesResponseBanner.TermNamesInView.GetValueOrDefault(CurrentTerm.Name, string.Empty);

            SetBannerCodeType();

            if ((IsEligible && ChildIsTooYoung) || (IsNotValidYet && ChildIsTooYoung)) // Child too young
            {

                DateTime nineMonthsDate = ChildDateOfBirth.AddMonths(9);
                CodeStatus = WorkingFamiliesResponseBanner.CodeChildTooYoung;
                BannerColour = WorkingFamiliesResponseBanner.ColourBlue;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFrom} {nextTermView} {nineMonthsDate.Year}";

            }

            else if (ChildIsTooOld || IsExpired) // Expired or too old
            {

                CodeStatus = WorkingFamiliesResponseBanner.CodeExpired;
                BannerColour = WorkingFamiliesResponseBanner.ColourOrange;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermExpiredOn} {GracePeriodEndDisplay}";
            }
            else if (IsNotValidYet) // Code cannot be used yet
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeNotValidYet;
                BannerColour = WorkingFamiliesResponseBanner.ColourBlue;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFrom} {nextTermView} {Response.GracePeriodEndDate.Year}";

            }

            // is Valid and reconfirmation has happened
            else if (IsReconfirmed)
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeValid;
                BannerColour = WorkingFamiliesResponseBanner.ColourGreen;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFor} {currentTermView} {DateTime.UtcNow.Year} and {nextTermView} {Response.GracePeriodEndDate.Year}";
            }

            else if (IsInGracePeriod)
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeInGracePeriod;
                BannerColour = WorkingFamiliesResponseBanner.ColourYellow;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermExpiresOn} {Response.GracePeriodEndDate:dd MMMM yyyy}";
            }
            else
            {
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFor} {currentTermView} {DateTime.UtcNow.Year}";
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