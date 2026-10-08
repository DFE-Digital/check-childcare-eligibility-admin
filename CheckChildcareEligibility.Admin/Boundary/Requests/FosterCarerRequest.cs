namespace CheckChildcareEligibility.Admin.Boundary.Requests
{
    public class FosterCarerRequest
    {
        public string CarerFirstName { get; set; }
        public string CarerLastName { get; set; }
        public DateTime CarerDateOfBirth { get; set; }
        public int? LocalAuthorityID { get; set; }
        public string? CarerNationalInsuranceNumber { get; set; }
        public bool? HasPartner { get; set; }
    }
}
