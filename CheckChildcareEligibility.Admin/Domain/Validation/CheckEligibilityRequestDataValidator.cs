// Ignore Spelling: Validator

using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.API.Domain.Validation;
using FluentValidation;

namespace CheckChildcareEligibility.Admin.Domain.Validation;

public class CheckEligibilityRequestDataValidator : AbstractValidator<CheckEligibilityRequestData>
{
    public CheckEligibilityRequestDataValidator()
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationMessages.RequiredLastName)
            .Must((x, lastName) => string.IsNullOrEmpty(lastName) || DataValidation.BeAValidName(lastName))
            .WithMessage(ValidationMessages.ValidLastName);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage(ValidationMessages.RequiredDOB)
            .Must((x, dob) => string.IsNullOrEmpty(dob) || DataValidation.BeAValidDate(dob))
            .WithMessage(ValidationMessages.ValidDOB);


        RuleFor(x => x.NationalInsuranceNumber)
            .NotEmpty().WithMessage(ValidationMessages.RequiredNI)
            .Must((x, NINumber) => string.IsNullOrEmpty(NINumber) || DataValidation.BeAValidNi(NINumber))
            .WithMessage(ValidationMessages.ValidNI);
    }
}