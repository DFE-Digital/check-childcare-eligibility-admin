namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class BulkCheckStatusViewModel
    {
        public string BulkCheckID { get; set; }
        public string Filename { get; set; } = string.Empty;
        public string EligibilityType { get; set; } = string.Empty;
        public DateTime DateSubmitted { get; set; }
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
