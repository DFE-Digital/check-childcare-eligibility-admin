using CheckChildcareEligibility.Admin.Domain.Constants.ResponseBanner;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class WFResponseViewModel
    {
        public string EligibilityCode { get; set; }
        public bool ChildIsTooYoung { get; set; }
        public bool ChildIsTooOld { get; set; }
        public bool IsEligible { get; set; }
        public bool IsVSDinFuture => ValidityStartDate.Month > DateTime.UtcNow.Month;
        public bool IsInGracePeriod => DateTime.UtcNow > ValidityEndDate && DateTime.UtcNow < GracePeriodEndDate;
        public bool IsExpired => DateTime.UtcNow > GracePeriodEndDate;
        public DateTime ValidityStartDate { get; set; }
        public DateTime ValidityEndDate { get; set;}
        public DateTime GracePeriodEndDate { get; set; }

        public string Term {
            get {
                int month = ValidityStartDate.Month;
                if (month >= 1 && month <= 3)
                    return ResponseBanner.SpringTerm;
                else if (month >= 4 && month <= 8)
                    return ResponseBanner.SummerTerm;
                else return ResponseBanner.AutumnTerm;
            }
        }
        
    }
}
