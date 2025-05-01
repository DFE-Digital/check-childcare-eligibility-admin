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
            .NotEmpty().WithMessage(ValidationMessages.LastName);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(DataValidation.BeAValidDate)
            .WithMessage(ValidationMessages.DOB);

            RuleFor(x => x.NationalInsuranceNumber)
                .NotEmpty()
                .Must(DataValidation.BeAValidNi)
                .WithMessage(ValidationMessages.NI);
    }
}