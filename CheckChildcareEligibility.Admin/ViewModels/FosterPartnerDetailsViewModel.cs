using CheckChildcareEligibility.Admin.Attributes;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterPartnerDetailsViewModel
    {
        [Name]
        [Required(ErrorMessage = FosterFamilyValidationMessages.PartnerFirstNameEmpty)]
        public string PartnerFirstName { get; set; }

        [Name]
        [Required(ErrorMessage = FosterFamilyValidationMessages.PartnerLastNameEmpty)]
        public string PartnerLastName { get; set; }

        [NotMapped]
        [Dob("date of birth", "partner", null, "Day", "Month", "Year")]
        public DateTime PartnerDateOfBirth { get; set; }
        public string? Day { get; set; }
        public string? Month { get; set; }
        public string? Year { get; set; }

        [Required(ErrorMessage = FosterFamilyValidationMessages.PartnerNationalInsuranceNumberEmpty)]
        [NinValidator]
        [MaxLength(13)]
        public string PartnerNationalInsuranceNumber { get; set; }
    }
}