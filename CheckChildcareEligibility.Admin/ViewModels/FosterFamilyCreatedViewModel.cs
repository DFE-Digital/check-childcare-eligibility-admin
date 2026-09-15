
using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterFamilyCreatedViewModel
    {
        public Guid FosterCarerId { get; init; }

        public Guid FosterChildId { get; init; }

        public string ChildName { get; init; }

        public Term ValidFromTerm { get; init; }

        public string EligibilityCode { get; init; }

        public DateTime ValidityStartDate { get; init; }

        public DateTime ReconfirmBetweenStart { get; init; }

        public DateTime ReconfirmBetweenEnd { get; init; }

        public DateTime GracePeriodEndDate { get; init; }
    }
}
