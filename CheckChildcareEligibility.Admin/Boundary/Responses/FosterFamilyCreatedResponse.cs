namespace CheckChildcareEligibility.Admin.Boundary.Responses
{
    public class FosterFamilyCreatedResponse : EligibilityCodeResponse
    {
        public Guid FosterCarerId { get; init; }
        
        public Guid FosterChildId { get; init; }
    }
}
