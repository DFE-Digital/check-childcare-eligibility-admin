namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class EligibilityOutcomeViewModel
    {
        public string EligibilityType { get; set; } = string.Empty;
        public string EligibilityTypeLabel { get; set; } = string.Empty;

        public string ParentLastName{ get; set; } = string.Empty;
        public string ParentDateOfBirth { get; set; } = string.Empty;
        public string ParentNino { get; set; } = string.Empty;
    }
}
