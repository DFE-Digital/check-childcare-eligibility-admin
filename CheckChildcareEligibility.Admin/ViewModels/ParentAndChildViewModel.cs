using CheckChildcareEligibility.Admin.Attributes;
using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class ParentAndChildViewModel
    {
        [MaxLength(11)]
        public string EligibilityCode { get; set; } = string.Empty;

        public string? NationalInsuranceNumber { get; set; }

        public string? ChildDateOfBirth { get; set; }

        public string? Day { get; set; }

        public string? Month { get; set; }

        public string? Year { get; set; }
    }
}