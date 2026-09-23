namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterCodePreviewViewModel
    {

        public DateTime EligibilityConfirmed { get; init; }
        
        public DateTime ReconfirmBetweenStart { get; init; }
        
        public DateTime ReconfirmBetweenEnd { get; init; }

        public DateTime GracePeriodEndDate { get; init; }

    }
}