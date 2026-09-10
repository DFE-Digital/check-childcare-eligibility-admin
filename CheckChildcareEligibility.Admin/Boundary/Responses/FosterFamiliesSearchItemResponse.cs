namespace CheckChildcareEligibility.Admin.Boundary.Responses
{
    public class FosterFamiliesSearchItemResponse
    {
        public string ChildName { get; set; } = string.Empty;

        public DateTime ChildDateOfBirth { get; set; }

        public string EligibilityCode { get; set; } = string.Empty;

        public string CarerName { get; set; } = string.Empty;

        public Guid FosterCarerId { get; set; }

        public Guid FosterChildId { get; set; }

        public DateTime EligibilityConfirmedOn { get; set; }

        public DateTime ReconfirmBetweenStart { get; set; }

        public DateTime ReconfirmBetweenEnd { get; set; }

        public DateTime GracePeriodEndDate { get; set; }

        public string ReconfirmationStatus { get; set; } = string.Empty;
    }
}
