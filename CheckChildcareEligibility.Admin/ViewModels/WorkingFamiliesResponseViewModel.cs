using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class WorkingFamiliesResponseViewModel
    {
        public CheckEligibilityItemWorkingFamilies Response { get; set; }
        public bool ChildIsTooYoung { get; set; }
        public bool ChildIsTooOld { get; set; }
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

        public string Term {
            get {
                int month = 
                    ValidityEndDate.Month;
                if (month >= 1 && month <= 3)
                    return WorkingFamiliesResponseBanner.SpringTerm;
                else if (month >= 4 && month <= 8)
                    return WorkingFamiliesResponseBanner.SummerTerm;
                else return WorkingFamiliesResponseBanner.AutumnTerm;
            }
        }
        
    }
}
