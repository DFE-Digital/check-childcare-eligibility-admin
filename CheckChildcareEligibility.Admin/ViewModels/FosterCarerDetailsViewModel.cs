using CheckChildcareEligibility.Admin.Attributes;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterCarerDetailsViewModel
    {
        [Name]
        [Required(ErrorMessage = FosterFamilyValidationMessages.CarerFirstNameEmpty)]
        public string CarerFirstName { get; set; }

        [Name]
        [Required(ErrorMessage = FosterFamilyValidationMessages.CarerLastNameEmpty)]
        public string CarerLastName { get; set; }

        [NotMapped]
        [Dob("date of birth", "foster carer", null, "Day", "Month", "Year")]
        public DateTime CarerDateOfBirth { get; set; }
        public string? Day { get; set; }
        public string? Month { get; set; }
        public string? Year { get; set; }

        [Required(ErrorMessage = FosterFamilyValidationMessages.CarerNationalInsuranceNumberEmpty)]
        [NinValidator]
        [MaxLength(13)]
        public string CarerNationalInsuranceNumber { get; set; }

        [Required(ErrorMessage = FosterFamilyValidationMessages.HasPartner)]
        public bool? HasPartner { get; set; }
    }
}