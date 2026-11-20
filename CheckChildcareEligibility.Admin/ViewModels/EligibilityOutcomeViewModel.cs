namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class EligibilityOutcomeViewModel
    {
        public string EligibilityType { get; set; } = string.Empty;

        public string EligibilityTypeLabel { get; set; } = string.Empty;

        public string ParentLastName { get; set; } = string.Empty;

        public string ParentDateOfBirth { get; set; } = string.Empty;

        public string ParentNino { get; set; } = string.Empty;

        public string ParentNass { get; set; } = string.Empty;

        public IdentifierOutcomeView IdentifierOutcomeView { get; set; }
    }

    public class IdentifierOutcomeView
    {
        public IdentifierOutcomeView(string eligibilityType, string? nino, string? nass)
        {
            var (identifierLabel, identifierValue) = eligibilityType switch
            {
                "EYPP" when string.IsNullOrEmpty(nino) => ("Asylum support reference number", $"{nass}"),
                _ => ("National Insurance number", $"{nino}")
            };

            IdentifierLabel = identifierLabel;
            IdentifierValue = identifierValue;
        }

        public string IdentifierLabel { get; set; }

        public string IdentifierValue { get; set; }
    }
}