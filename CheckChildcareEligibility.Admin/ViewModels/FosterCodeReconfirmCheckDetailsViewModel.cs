using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterCodeReconfirmCheckDetailsViewModel
    {
        public Guid FosterChildId { get; set; }

        public string ChildFullName { get; set; }

        public FosterCodeReconfirmDateViewModel FosterCodeReconfirmDateViewModel { get; set; } = new();
        
        public FosterCodePreviewResponse FosterCodePreview { get; set; } = new();

    }
}