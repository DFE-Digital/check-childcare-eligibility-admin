using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterApplicationCheckDetailsViewModel
    {
        public string? ContextId { get; set; }
        public FosterCarerDetailsViewModel FosterCarerDetailsViewModel { get; set; } = new();
        public FosterPartnerDetailsViewModel? FosterPartnerDetailsViewModel { get; set; }
        public FosterChildDetailsViewModel FosterChildDetailsViewModel { get; set; } = new();
        public FosterApplicationSubmittedDateViewModel FosterApplicationSubmittedDateViewModel { get; set; } = new();
        public FosterFamilyCodePreviewResponse FosterCodePreview { get; set; } = new();
    }
}