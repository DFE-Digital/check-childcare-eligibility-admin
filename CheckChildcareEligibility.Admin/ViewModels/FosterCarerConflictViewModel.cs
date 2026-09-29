using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterCarerConflictViewModel
    {

        public FosterCarerDetailsViewModel CarerDetails { get; set; }

        public FosterFamilyResponse ConflictingFamily {get;set;}

    }
}