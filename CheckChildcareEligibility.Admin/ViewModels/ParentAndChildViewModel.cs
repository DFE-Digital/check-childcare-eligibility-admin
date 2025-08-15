using CheckChildcareEligibility.Admin.Attributes;
using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class ParentAndChildViewModel
    {
        [Nino]
        [MaxLength(13)]
        public string? NationalInsuranceNumber { get; set; }
        public string? ChildDateOfBirth { get; set; }
        public Child Child { get; set; } = new Child();
    }
}