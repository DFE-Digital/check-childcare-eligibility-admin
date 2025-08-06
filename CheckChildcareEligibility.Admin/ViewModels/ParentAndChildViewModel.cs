using CheckChildcareEligibility.Admin.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class ParentAndChildViewModel
    {
        public string EligibilityType { get; set; } = string.Empty;
        public string EligibilityTypeLabel { get; set; } = string.Empty;

        [MaxLength(11)] public string EligibilityCode { get; set; } = string.Empty;

        [Nino][MaxLength(13)] public string? NationalInsuranceNumber { get; set; }

        public string? ChildDateOfBirth { get; set; }

        public string? Day { get; set; }

        public string? Month { get; set; }

        public string? Year { get; set; }
    }
}