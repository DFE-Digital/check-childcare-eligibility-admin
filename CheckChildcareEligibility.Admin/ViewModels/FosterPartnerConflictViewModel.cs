using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterPartnerConflictViewModel
    {
        public FosterPartnerDetailsViewModel PartnerDetails { get; set; }

        public FosterFamilyResponse ConflictingFamily { get; set; }
    }
}