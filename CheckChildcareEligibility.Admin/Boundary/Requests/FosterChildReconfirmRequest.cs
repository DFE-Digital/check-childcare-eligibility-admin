namespace CheckChildcareEligibility.Admin.Boundary.Requests
{
    public class FosterChildReconfirmRequest
    {
        public string EligibilityCode { get; set; }

        public DateTime SubmissionDate { get; set; }
    }
}