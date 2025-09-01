using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class WorkingFamiliesResponseViewModel
    {
        public CheckEligibilityItemWorkingFamilies Response { get; set; }
        public bool ChildIsTooYoung => ValidityStartDate < ChildDateOfBirth.AddMonths(9); public bool ChildIsTooOld { get; set; }
        public bool IsEligible => Response.Status == CheckEligibilityStatus.eligible.ToString();
        public bool IsVSDinFuture => ValidityStartDate.Month > DateTime.UtcNow.Month;
        public bool IsInGracePeriod => DateTime.UtcNow > ValidityEndDate && DateTime.UtcNow < GracePeriodEndDate;
        public bool IsExpired => DateTime.UtcNow > GracePeriodEndDate;
        public bool IsTemporaryCode => Response.EligibilityCode.StartsWith("1");
        public bool IsFosterCode => Response.EligibilityCode.StartsWith("4");
        public DateTime ValidityStartDate => DateTime.Parse(Response.ValidityStartDate);
        public DateTime ValidityEndDate => DateTime.Parse(Response.ValidityEndDate);
        public DateTime GracePeriodEndDate => DateTime.Parse(Response.GracePeriodEndDate);
        public DateTime ChildDateOfBirth => DateTime.Parse(Response.DateOfBirth);

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
    }
}
