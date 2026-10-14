namespace CheckChildcareEligibility.Admin.Boundary.Requests
{
    public class FosterChildRequest
    {
        public string ChildFirstName { get; set; } = string.Empty;
        
        public string ChildLastName { get; set; } = string.Empty;

        public DateTime ChildDateOfBirth { get; set; }

        public string ChildPostCode { get; set; } = string.Empty;

        public DateTime? SubmissionDate { get; set; }

    }
}
