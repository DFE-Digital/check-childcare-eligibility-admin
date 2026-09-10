using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Helpers;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class WorkingFamiliesResponseViewModel
    {

        public CheckEligibilityItemWorkingFamilies Response { get; set; }
        public bool ChildIsTooYoung => WorkingFamiliesCheckHelper.ChildIsTooYoung(Response, DateTime.UtcNow);
        public bool ChildIsTooOld => WorkingFamiliesCheckHelper.ChildIsTooOld(Response, DateTime.UtcNow);
        public bool IsEligible => Response.Status == CheckEligibilityStatus.eligible.ToString();
        public bool IsInGracePeriod => WorkingFamiliesCheckHelper.IsInGracePeriod(Response, DateTime.UtcNow);
        public bool IsExpired => WorkingFamiliesCheckHelper.IsExpired(Response, DateTime.UtcNow);
        public bool IsTemporaryCode => WorkingFamiliesCheckHelper.IsTemporaryCode(Response);
        public bool IsFosterCode => WorkingFamiliesCheckHelper.IsFosterCode(Response);
        public DateTime ValidityStartDate => DateTime.Parse(Response.ValidityStartDate);
        public DateTime ValidityEndDate => DateTime.Parse(Response.ValidityEndDate);
        public DateTime GracePeriodEndDate => DateTime.Parse(Response.GracePeriodEndDate);
        public DateTime GetTermStart(DateTime date) => WorkingFamiliesCheckHelper.GetTermStart(date);

        public string GracePeriodEndDisplay =>
            (IsEligible && ChildIsTooYoung) || IsNotValidYet
                ? WorkingFamiliesResponseDetails.GracePeriodEndDateNotAvailable
                : GracePeriodEndDate.ToString("d MMMM yyyy");
        public string ReconfirmationDateLabel =>
            IsTemporaryCode
                ? "Apply for a new code by"
                : "Reconfirm between";

        public string ReconfirmationDateDisplay
        {
            get
            {
                if (IsTemporaryCode)
                {
                    return ValidityEndDate.ToString("d MMMM yyyy");
                }

                if (ChildIsTooOld)
                {
                    return WorkingFamiliesResponseDetails.StatusNotApplicable;
                }

                return $"{StartReconfirmDate:d MMMM yyyy} and {ValidityEndDate:d MMMM yyyy}";
            }
        }

        public DateTime ChildDateOfBirth => DateTime.Parse(Response.DateOfBirth);
        private DateTime StartReconfirmDate => ValidityEndDate.AddDays(-28);
        public int CurrentYear => DateTime.UtcNow.Year;
        public string CodeType = WorkingFamiliesResponseBanner.CodePermanent;
        public string CodeStatus = WorkingFamiliesResponseBanner.CodeValid;
        public string BannerColour = WorkingFamiliesResponseBanner.ColourGreen;
        public string TermValidityDetails = WorkingFamiliesResponseBanner.TermValidFor;
        public string TermInfo => GetTermInfo(ValidityStartDate);
        public string CurrentTerm => WorkingFamiliesCheckHelper.GetTermName(DateTime.UtcNow);
        public string NextTerm => WorkingFamiliesCheckHelper.GetTermName(
            WorkingFamiliesCheckHelper.GetNextTerm(WorkingFamiliesCheckHelper.GetTermStart(DateTime.UtcNow)));

        public string GetTermInfo(DateTime vsd)
        {
            return WorkingFamiliesCheckHelper.GetTermInfo(Response, vsd, DateTime.UtcNow);
        }

        public bool IsReconfirmed
        {
            get
            {
                string endTermName = WorkingFamiliesCheckHelper.GetTermName(WorkingFamiliesCheckHelper.GetTermStart(GracePeriodEndDate));
                return IsEligible && !IsNotValidYet && !IsInGracePeriod && CurrentTerm != endTermName;
            }
        }

        public bool IsNotValidYet
        {
            get
            {
                return WorkingFamiliesCheckHelper.IsNotValidYet(Response, DateTime.UtcNow);
            }
        }

        public string GetBannerCodeType()
        {

            if (ChildIsTooOld)//child too old - Child has reached compulsory school age
            {
                return WorkingFamiliesResponseBanner.ReconfirmationChildTooOld;
            }
            else if (DateTime.Now >= StartReconfirmDate && DateTime.Now <= ValidityEndDate) //due now
            {
                return $"{WorkingFamiliesResponseBanner.ReconfirmationBefore} {ValidityEndDate.ToString("d MMMM yyyy")}";
            }
            else if (DateTime.Now > ValidityEndDate) //overdue - Needs reconfirming now
            {
                return WorkingFamiliesResponseBanner.ReconfirmationOverdue;
            }
            return string.Empty;
        }

        public void SetBannerValues()
        {
            if (IsTemporaryCode)
            {
                CodeType = WorkingFamiliesResponseBanner.CodeTemporary;

                if (IsEligible)
                {
                    TermValidityDetails = "Only "  + TermValidityDetails;
                }
            }
            else if (IsFosterCode)
            {
                CodeType = WorkingFamiliesResponseBanner.CodeFosterFamily;
            }

            if (IsEligible && ChildIsTooYoung) // Child too young
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeChildTooYoung;
                BannerColour = WorkingFamiliesResponseBanner.ColourBlue;
                TermValidityDetails = WorkingFamiliesResponseBanner.TermValidFrom;
                CodeType = string.Empty;
            }
            else if (IsNotValidYet) // Code cannot be used yet
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeNotValidYet;
                BannerColour = WorkingFamiliesResponseBanner.ColourBlue;
                TermValidityDetails = WorkingFamiliesResponseBanner.TermValidFrom;
            }
            else if (IsExpired) // Expired
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeExpired;
                BannerColour = WorkingFamiliesResponseBanner.ColourOrange;
                TermValidityDetails = WorkingFamiliesResponseBanner.TermExpiredOn;
            }
            else if (IsInGracePeriod) // Code is in grace period
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeInGracePeriod;
                BannerColour = WorkingFamiliesResponseBanner.ColourYellow;
                TermValidityDetails = WorkingFamiliesResponseBanner.TermExpiresOn;
            }
            else if (IsReconfirmed)
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeValid;
                BannerColour = WorkingFamiliesResponseBanner.ColourGreen;
                TermValidityDetails = WorkingFamiliesResponseBanner.TermValidFor + " " + CurrentTerm + " and " + NextTerm;
            }
        }

        public string GetBannerReconfirmationMessage()
        {
            if ((IsTemporaryCode && CodeStatus == WorkingFamiliesResponseBanner.CodeValid)
                || (IsTemporaryCode && CodeStatus == WorkingFamiliesResponseBanner.CodeInGracePeriod)
                || (IsTemporaryCode && CodeStatus == WorkingFamiliesResponseBanner.CodeExpired))
            {
                return string.Empty;
            }
            else if (WorkingFamiliesCheckHelper.HasReachedCompulsorySchoolAge(ChildDateOfBirth, DateTime.Now))//child too old - Child has reached compulsory school age
            {
                return WorkingFamiliesResponseBanner.ReconfirmationChildTooOld;
            }
            else if (DateTime.Now >= StartReconfirmDate && DateTime.Now <= ValidityEndDate) //due now
            {
                return $"{WorkingFamiliesResponseBanner.ReconfirmationBefore} {ValidityEndDate.ToString("d MMMM yyyy")}";
            }
            else if (DateTime.Now > ValidityEndDate) //overdue - Needs reconfirming now
            {
                return WorkingFamiliesResponseBanner.ReconfirmationOverdue;
            }
            return string.Empty;
        }

        public string[] GetReconfirmationStatus()
        {
            if (IsTemporaryCode) //Temp code
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusNotApplicable;
            }
            else if (ChildIsTooOld)//child too old
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusChildTooOld;
            }
            else if (DateTime.Now < StartReconfirmDate)
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusNotDueYet;
            }
            else if (DateTime.Now >= StartReconfirmDate && DateTime.Now <= ValidityEndDate) //due now
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusDueNow;
            }
            else if (DateTime.Now > ValidityEndDate) //overdue - Needs reconfirming now
            {
                return WorkingFamiliesResponseDetails.ReconfirmationStatusOverdue;
            }
            return ["Not set", "purple"]; // Should not reach this
        }

    }
}