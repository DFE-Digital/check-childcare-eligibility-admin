namespace CheckChildcareEligibility.Admin.Models
{
    public class BulkCheck
    {
        public string Guid { get; set; }
        public string ClientIdentifier { get; set; } = string.Empty;
        public string Filename {  get; set; } = string.Empty;
        public string EligibilityType  { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
