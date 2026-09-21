using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;
using CheckChildcareEligibility.Admin.Helpers;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class WorkingFamiliesResponseViewModel
    {

        public WorkingFamiliesResponseViewModel(CheckEligibilityItemWorkingFamilies response, EligibilityCodeProperties properties)
        {
            Response = response;
            Properties = properties;
        }

        private CheckEligibilityItemWorkingFamilies Response { get; set; }

        public EligibilityCodeProperties Properties { get; set; }

        public string NationalInsuranceNumber => Response.NationalInsuranceNumber;
        public string EligibilityConfirmedOnDisplay =>
            $"{Response.ValidityStartDate:d MMMM yyyy}" +
            (Response.IsDiscretionaryValidityStartDateApplied == true
                ? " (discretionary start date applied)"
                : string.Empty);

        public string GracePeriodEndDisplay =>
            Properties.ChildIsTooOld
                ? Properties.ValidityEndDate.ToString("d MMMM yyyy")
                : (Properties.IsEligible && Properties.ChildIsTooYoung) || Properties.IsNotValidYet
                    ? WorkingFamiliesResponseDetails.GracePeriodEndDateNotAvailable
                    : Properties.GracePeriodEndDate.ToString("d MMMM yyyy");

        public string GracePeriodEndLabel =>
            Properties.ChildIsTooOld
                ? "Grace period ended"
                : "Grace period ends";

        public string ReconfirmationDateLabel =>
            Properties.Type == EligibilityCodeType.Temporary
                ? "Apply for a new code by"
                : "Reconfirm between";

        public string ReconfirmBetween => WorkingFamiliesCheckHelper.GetReconfirmBetween(
             Properties.Type,
             Properties.ReconfirmationProperties,
             Properties.ValidityEndDate
        );

        public string CodeType = string.Empty;
        public string CodeStatus = WorkingFamiliesResponseBanner.CodeValid;
        public string BannerColour = WorkingFamiliesResponseBanner.ColourGreen;
        public string TermValidityDetails = WorkingFamiliesResponseBanner.TermValidFor;

        public TermName NextTermName => (Properties.NextTerm?.Name ?? TermName.None);

        // If child is too young and the code has not expired do not set code type
        // else apply correct content for code type
        private void SetBannerCodeType()
        {
            if (Properties.ChildIsTooYoung && !Properties.IsExpired) return;
            if (Properties.Type == EligibilityCodeType.Temporary)
            {
                CodeType = WorkingFamiliesResponseBanner.CodeTemporary;

                if (Properties.IsEligible)
                {
                    TermValidityDetails = "Only " + TermValidityDetails;
                }
            }
            else if (Properties.Type == EligibilityCodeType.Foster)
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
            var nextTermView = WorkingFamiliesResponseBanner.TermNamesInView.GetValueOrDefault(Properties.NextTerm.Name, string.Empty);
            var currentTermView = WorkingFamiliesResponseBanner.TermNamesInView.GetValueOrDefault(Properties.CurrentTerm.Name, string.Empty);

            SetBannerCodeType();

            if ((Properties.IsEligible && Properties.ChildIsTooYoung) || (Properties.IsNotValidYet && Properties.ChildIsTooYoung)) // Child too young
            {
                DateTime nineMonthsDate = Properties.ChildDateOfBirth.AddMonths(9);
                CodeStatus = WorkingFamiliesResponseBanner.CodeChildTooYoung;
                BannerColour = WorkingFamiliesResponseBanner.ColourBlue;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFrom} {nextTermView} {nineMonthsDate.Year}";
            }
            else if (Properties.ChildIsTooOld || Properties.IsExpired) // Expired or too old
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeExpired;
                BannerColour = WorkingFamiliesResponseBanner.ColourOrange;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermExpiredOn} {GracePeriodEndDisplay}";
            }
            else if (Properties.IsNotValidYet) // Code cannot be used yet
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeNotValidYet;
                BannerColour = WorkingFamiliesResponseBanner.ColourBlue;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFrom} {nextTermView} {Properties.GracePeriodEndDate.Year}";

            }

            // is Valid and reconfirmation has happened
            else if (Properties.IsReconfirmed)
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeValid;
                BannerColour = WorkingFamiliesResponseBanner.ColourGreen;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFor} {currentTermView} {DateTime.UtcNow.Year} and {nextTermView} {Properties.GracePeriodEndDate.Year}";
            }
            else if (Properties.IsInGracePeriod)
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeInGracePeriod;
                BannerColour = WorkingFamiliesResponseBanner.ColourYellow;
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermExpiresOn} {Properties.GracePeriodEndDate:dd MMMM yyyy}";
            }
            else
            {
                TermValidityDetails = $"{WorkingFamiliesResponseBanner.TermValidFor} {currentTermView} {DateTime.UtcNow.Year}";
            }
        }

        public string SetBannerReconfirmationMessage()
        {
            if (Properties.ReconfirmationProperties?.Status == ReconfirmationStatus.Due)
            {
                return $"{WorkingFamiliesResponseBanner.ReconfirmationBefore} {Properties.ReconfirmationProperties.EndDate?.ToString("d MMMM yyyy")}";
            }
            else if (Properties.ReconfirmationProperties?.Status == ReconfirmationStatus.Overdue)
            {
                return WorkingFamiliesResponseBanner.ReconfirmationOverdue;
            }
            else if (Properties.ReconfirmationProperties?.Status == ReconfirmationStatus.ChildTooOld)
            {
                return WorkingFamiliesResponseBanner.ReconfirmationChildTooOld;
            }
            return string.Empty;
        }

        public string[] SetReconfirmationStatus => WorkingFamiliesCheckHelper.GetReconfirmStatus(Properties.ReconfirmationProperties);

    }
}