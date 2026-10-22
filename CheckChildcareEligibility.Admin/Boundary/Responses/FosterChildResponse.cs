namespace CheckChildcareEligibility.Admin.Boundary.Responses
{
    public class FosterChildResponse
    {
        // Eligibility Code Details

        public string EligibilityCode { get; set; } = string.Empty;

        public DateTime ValidityStartDate { get; set; }

        public DateTime ValidityEndDate { get; set; }

        public DateTime GracePeriodEndDate { get; set; }

        public bool IsGracePeriodEndDateApplied { get; set; }

        public ReconfirmationProperties ReconfirmationProperties { get; set; }

        public TermValidity TermValidity { get; set; }

        public bool ChildTooYoung { get; set; }

        public bool ChildTooOld { get; set; }

        // Child
        public Guid FosterChildId { get; set; }

        public string ChildFirstName { get; set; } = string.Empty;

        public string ChildLastName { get; set; } = string.Empty;

        public DateTime ChildDateOfBirth { get; set; }

        public string ChildPostCode { get; set; } = string.Empty;


        // Foster Family
        public Guid FosterCarerId { get; set; }

        public string? CarerName { get; set; }

        public string? PartnerName { get; set; }
    }
}
