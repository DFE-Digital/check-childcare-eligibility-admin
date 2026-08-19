using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterFamilyCreatedViewModel : EligibilityCodeViewModel
    {
            public Guid FosterCarerId { get; init; }
            public string ChildName { get; init; }
    }
}
