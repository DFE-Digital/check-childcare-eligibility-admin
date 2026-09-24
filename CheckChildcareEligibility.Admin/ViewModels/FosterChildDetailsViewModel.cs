using CheckChildcareEligibility.Admin.Attributes;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterChildDetailsViewModel
    {
        [Name]
        [Required(ErrorMessage = FosterFamilyValidationMessages.ChildFirstNameEmpty)]
        public string ChildFirstName { get; set; }

        [Name]
        [Required(ErrorMessage = FosterFamilyValidationMessages.ChildLastNameEmpty)]
        public string ChildLastName { get; set; }

        [NotMapped]
        [Dob("date of birth", "child", null, "Day", "Month", "Year")]
        public DateTime ChildDateOfBirth { get; set; }
        public string? Day { get; set; }
        public string? Month { get; set; }
        public string? Year { get; set; }

        [PostCode]
        [Required(ErrorMessage = FosterFamilyValidationMessages.ChildPostCodeEmpty)]
        public string ChildPostCode { get; set; }

        //For Update Child Details only
        public Guid FosterChildId { get; set; }

        // For creation journey only
        public string? ContextId { get; set; }

        public bool HasPartner { get; set; }

        public FosterChildRequest BuildRequest()
        {
            return new FosterChildRequest
            {
                ChildFirstName = ChildFirstName,
                ChildLastName = ChildLastName,
                ChildDateOfBirth = ChildDateOfBirth,
                ChildPostCode = ChildPostCode
            };
        }
    }
}