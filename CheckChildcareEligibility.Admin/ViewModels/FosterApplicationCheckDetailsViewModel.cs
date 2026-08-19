namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterApplicationCheckDetailsViewModel
    {

        public FosterCarerDetailsViewModel fosterCarerDetailsViewModel { get; set; } = new();
        public FosterPartnerDetailsViewModel fosterPartnerDetailsViewModel { get; set; } = new();
        public FosterChildDetailsViewModel fosterChildDetailsViewModel { get; set; } = new();
        public FosterApplicationSubmittedDateViewModel fosterApplicationSubmittedDateViewModel { get; set; } = new();
    }
}