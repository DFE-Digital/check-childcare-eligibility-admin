using CheckChildcareEligibility.Admin.Boundary.Responses;
namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterFamilyViewModel
    {
        public FosterFamilyResponse Response { get; set; }

        public bool CarerUpdated { get; set; }
        
        public bool PartnerUpdated { get; set; }

        public bool ChildUpdated { get; set; }
    }
}