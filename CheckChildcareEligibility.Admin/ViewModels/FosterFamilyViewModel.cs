using CheckChildcareEligibility.Admin.Boundary.Responses;
namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterFamilyViewModel
    {
        public FosterFamilyResponse Response { get; set; }

        public string? Confirmation { get; set; }
        
    }
}