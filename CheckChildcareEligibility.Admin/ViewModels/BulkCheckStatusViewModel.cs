namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class BulkCheckStatusViewModel
    {
        public string Guid { get; set; }
        public string ClientIdentifier { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string EligibilityType { get; set; } = string.Empty;
        public string DateSubmitted { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class BulkCheckStatusesViewModel
    {
        public BulkCheckStatusesViewModel()
        {
            Checks = new List<BulkCheckStatusViewModel>();
        }

        public List<BulkCheckStatusViewModel> Checks { get; set; }

    }
}
