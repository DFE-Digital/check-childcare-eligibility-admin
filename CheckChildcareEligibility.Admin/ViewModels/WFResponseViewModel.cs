using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class WFResponseViewModel
    {
        public CheckEligibilityItemWorkingFamilies Response { get; set; }
        public bool ChildIsTooYoung { get; set; }
        public bool ChildIsTooOld { get; set; }
        public bool IsEligible => Response.Status == CheckEligibilityStatus.eligible.ToString();
        public bool IsVSDinFuture => ValidityStartDate.Month > DateTime.UtcNow.Month;
        public bool IsInGracePeriod => DateTime.UtcNow > ValidityEndDate && DateTime.UtcNow < GracePeriodEndDate;
        public bool IsExpired => DateTime.UtcNow > GracePeriodEndDate;
        public bool isTemporaryCode => Response.EligibilityCode.StartsWith("4");
        public DateTime ValidityStartDate { get; set; }
        public DateTime ValidityEndDate { get; set;}
        public DateTime GracePeriodEndDate { get; set; }

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
