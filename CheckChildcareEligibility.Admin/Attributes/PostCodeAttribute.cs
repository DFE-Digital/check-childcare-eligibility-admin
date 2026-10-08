using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.API.Domain.Validation;
using System.ComponentModel.DataAnnotations;

namespace CheckChildcareEligibility.Admin.Attributes
{
    public class PostCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var postCode = value?.ToString();

            if (string.IsNullOrEmpty(postCode))
                return ValidationResult.Success;

            if (!DataValidation.BeAValidUkPostcode(postCode))
            {
                var constantPostCode = $"{validationContext.MemberName}Invalid";
                var field = typeof(FosterFamilyValidationMessages).GetField(constantPostCode);

                if (field != null)
                {
                    var message = field.GetValue(null)?.ToString();
                    return new ValidationResult(message);
                }

                return new ValidationResult("Enter a full UK postcode");
            }

            return ValidationResult.Success;
        }
    }
}
