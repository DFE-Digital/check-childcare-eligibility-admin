namespace CheckChildcareEligibility.Admin.Boundary.Responses
{
    public class FosterFamilyCodePreviewResponse
    {
        public DateTime ValidityStartDate { get; init; }

        public string ValidFromTerm { get; init; } = string.Empty;

        public DateTime EligibilityConfirmed { get; init; }

        public DateTime ReconfirmBetweenStart { get; init; }
        
        public DateTime ReconfirmBetweenEnd { get; init; }
        
        public DateTime GracePeriodEndDate { get; init; }
    }
}