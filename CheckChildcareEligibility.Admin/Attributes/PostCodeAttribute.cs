using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckChildcareEligibility.Admin.Attributes
{
    public class PostCodeAttribute : ValidationAttribute
    {
        public static readonly string PostCodeValidationRegex = @"^([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z])))) [0-9][A-Za-z]{2})$";
        private static readonly Regex regex = new(PostCodeValidationRegex);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance;

            var postCodeProperty = model.GetType()
                                .GetProperties()
                                .FirstOrDefault(p => p.Name.Contains("PostCode"));

            var postCode = postCodeProperty?.GetValue(model)?.ToString() ?? string.Empty;

            if (postCode == value)
            {
                if (value == null || value == "")
                    return ValidationResult.Success;

                if (!regex.IsMatch(value.ToString()))
                {
                    var constantPostCode = $"{postCodeProperty.Name}Invalid";
                    var field = typeof(FosterFamilyValidationMessages).GetField(constantPostCode);
                    if (field != null)
                    {
                        var message = field?.GetValue(null)?.ToString();
                        return new ValidationResult(message);
                    }
                    return new ValidationResult("Enter a full UK postcode");
                }
            }

            return ValidationResult.Success;
        }
    }
}
