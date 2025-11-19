// Ignore Spelling: Validator

using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.API.Domain.Validation;
using FluentValidation;

namespace CheckChildcareEligibility.Admin.Domain.Validation;

public class CheckEligibilityRequestDataValidator : AbstractValidator<IEligibilityServiceType>
{
    public CheckEligibilityRequestDataValidator()
    {
        When(x => x is CheckEligibilityRequestData, () =>
        {
            RuleFor(x => ((CheckEligibilityRequestData)x).LastName)
           .NotEmpty().WithMessage(ValidationMessages.RequiredLastName)
           .Must((x, lastName) => string.IsNullOrEmpty(lastName) || DataValidation.BeAValidName(lastName))
           .WithMessage(ValidationMessages.ValidLastName);

            RuleFor(x => ((CheckEligibilityRequestData)x).DateOfBirth)
                .NotEmpty().WithMessage(ValidationMessages.RequiredDOB)
                .Must((x, dob) => string.IsNullOrEmpty(dob) || DataValidation.BeAValidDate(dob))
                .WithMessage(ValidationMessages.ValidDOB);

            RuleFor(x => ((CheckEligibilityRequestData)x).NationalInsuranceNumber)
                .NotEmpty().WithMessage(ValidationMessages.RequiredNI)
                .Must((x, NINumber) => string.IsNullOrEmpty(NINumber) || DataValidation.BeAValidNi(NINumber))
                .WithMessage(ValidationMessages.ValidNI);
        });

        When(x => x is CheckEligibilityRequestWorkingFamiliesData, () =>
        {
            RuleFor(x => ((CheckEligibilityRequestWorkingFamiliesData)x).NationalInsuranceNumber)
             .NotEmpty().WithMessage(ValidationMessages.RequiredNI)
             .Must((x, NINumber) => string.IsNullOrEmpty(NINumber) || DataValidation.BeAValidNi(NINumber))
             .WithMessage(ValidationMessages.ValidNI);

            RuleFor(x => ((CheckEligibilityRequestWorkingFamiliesData)x).DateOfBirth)
                .NotEmpty().WithMessage(ValidationMessages.RequiredDOB)
                .Must((x, dob) => string.IsNullOrEmpty(dob) || DataValidation.BeAValidDate(dob))
                .WithMessage(ValidationMessages.ValidDOB);

            RuleFor(x => ((CheckEligibilityRequestWorkingFamiliesData)x).EligibilityCode)
                .NotEmpty().WithMessage(ValidationMessages.EligibilityCodeNullOrEmpty)
                .Must(x => !long.TryParse(x, out _)).WithMessage(ValidationMessages.EligibilityCodeNumber)
                .Must(x => x.Length != 11 ).WithMessage(ValidationMessages.EligibilityCodeIncorrectLength);
        });

    }
}