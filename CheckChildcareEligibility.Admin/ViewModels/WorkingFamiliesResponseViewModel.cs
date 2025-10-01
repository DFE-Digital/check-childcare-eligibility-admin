using Azure.Storage.Blobs.Models;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CsvHelper;
using System.Reflection;
using System.Reflection.Emit;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class WorkingFamiliesResponseViewModel
    {
        public CheckEligibilityItemWorkingFamilies Response { get; set; }
        public bool ChildIsTooYoung => ValidityStartDate < ChildDateOfBirth.AddMonths(9);
        public bool ChildIsTooOld => HasReachedCompulsorySchoolAge(ChildDateOfBirth, DateTime.UtcNow);
        public bool IsEligible => Response.Status == CheckEligibilityStatus.eligible.ToString();
        public bool IsVSDinFuture => ValidityStartDate > DateTime.UtcNow;
        public bool IsInGracePeriod => DateTime.UtcNow > ValidityEndDate && DateTime.UtcNow < GracePeriodEndDate;
        public bool IsExpired => DateTime.UtcNow > GracePeriodEndDate;
        public bool IsTemporaryCode => Response.EligibilityCode.StartsWith("1");
        public bool IsFosterCode => Response.EligibilityCode.StartsWith("4");
        public DateTime ValidityStartDate => DateTime.Parse(Response.ValidityStartDate);
        public DateTime ValidityEndDate => DateTime.Parse(Response.ValidityEndDate);
        public DateTime GracePeriodEndDate => DateTime.Parse(Response.GracePeriodEndDate);
        public DateTime ChildDateOfBirth => DateTime.Parse(Response.DateOfBirth);
        private DateTime StartReconfirmDate => ValidityEndDate.AddDays(-28);
        public int CurrentYear => DateTime.UtcNow.Year;
        public string CodeType = WorkingFamiliesResponseBanner.CodePermanent;
        public string CodeStatus = WorkingFamiliesResponseBanner.CodeValid;
        public string BannerColour = WorkingFamiliesResponseBanner.ColourGreen;
        public string TermValidityDetails = WorkingFamiliesResponseBanner.TermValidFor;


        public string Term
        {
            get
            {
                DateTime vsd = ValidityStartDate;
                DateTime nineMonthsDate = ChildDateOfBirth.AddMonths(9);

                if (ChildIsTooYoung && vsd < nineMonthsDate)
                {
                    vsd = nineMonthsDate;
                }

                if (DateTime.UtcNow > ValidityEndDate)
                {
                    return $"{GracePeriodEndDate:dd MMMM yyyy}";
                }

                string termName;
                if (vsd.Month >= 1 && vsd.Month <= 3)
                    termName = WorkingFamiliesResponseBanner.SpringTerm;
                else if (vsd.Month >= 4 && vsd.Month <= 8)
                    termName = WorkingFamiliesResponseBanner.SummerTerm;
                else
                    termName = WorkingFamiliesResponseBanner.AutumnTerm;

                return $"{termName} {vsd.Year}";
            }
            
        }

        public static DateTime GetTermStart(DateTime date)
        {
            int year = date.Year;

            if (date >= new DateTime(year, 9, 1))
                return new DateTime(year, 9, 1);
            else if (date >= new DateTime(year, 4, 1))
                return new DateTime(year, 4, 1);
            else
                return new DateTime(year, 1, 1);
        }

        public static DateTime GetNextTerm(DateTime termStart)
        {
            if (termStart.Month == 1)
                return new DateTime(termStart.Year, 4, 1);
            else if (termStart.Month == 4)
                return new DateTime(termStart.Year, 9, 1);
            else // September
                return new DateTime(termStart.Year + 1, 1, 1);
        }

        private bool HasReachedCompulsorySchoolAge(DateTime dateOfBirth, DateTime currentCheckDate)
        {
            DateTime fifthBirthday = dateOfBirth.AddYears(5);
            DateTime termChildTurnsFive = GetTermStart(fifthBirthday);
            DateTime termAfterFive = GetNextTerm(termChildTurnsFive);
        
            return currentCheckDate >= termAfterFive;
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
            else if (IsVSDinFuture) // Validity start date in future
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeVEDinFuture;
                BannerColour = WorkingFamiliesResponseBanner.ColourBlue;
            }
            else if (IsInGracePeriod) // Code is in grace period
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeInGracePeriod;
                BannerColour = WorkingFamiliesResponseBanner.ColourYellow;
                TermValidityDetails = WorkingFamiliesResponseBanner.TermExpiresOn;
            }
            else if (IsExpired) // Expired
            {
                CodeStatus = WorkingFamiliesResponseBanner.CodeExpired;
                BannerColour = WorkingFamiliesResponseBanner.ColourOrange;
                TermValidityDetails = WorkingFamiliesResponseBanner.TermExpiredOn;
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
            else if (HasReachedCompulsorySchoolAge(ChildDateOfBirth, DateTime.Now))//child too old - Child has reached compulsory school age
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