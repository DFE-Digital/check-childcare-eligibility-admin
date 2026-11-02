// Ignore Spelling: Validator

using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckYourEligibility.API.Domain.Validation;
using FluentValidation;
using System.Text.RegularExpressions;

namespace CheckChildcareEligibility.Admin.Domain.Validation;

public class CheckEligibilityRequestDataValidator : AbstractValidator<IEligibilityServiceType>
{
    public CheckEligibilityRequestDataValidator()
    {
        When(x => x is CheckEligibilityRequestDataBase, () =>
        {
            RuleFor(x => ((CheckEligibilityRequestDataBase)x).Reference)
                .MaximumLength(100).WithMessage(ValidationMessages.ReferenceTooLong);
        });

        When(x => x is CheckEligibilityRequestData, () =>
        {
            RuleFor(x => ((CheckEligibilityRequestData)x).LastName)
               .Cascade((CascadeMode.Stop))
               .NotEmpty().WithMessage(ValidationMessages.RequiredLastName)
               .Must((x, lastName) => string.IsNullOrEmpty(lastName) || DataValidation.BeAValidName(lastName))
               .WithMessage(ValidationMessages.InvalidLastName);

            RuleFor(x => ((CheckEligibilityRequestData)x).DateOfBirth)
                .Cascade((CascadeMode.Stop))
                .NotEmpty().WithMessage(ValidationMessages.RequiredDOB)
                .Must((x, dob) => string.IsNullOrEmpty(dob) || DataValidation.BeAValidDate(dob))

                .WithMessage(ValidationMessages.InvalidDOB);
            When(x => ((CheckEligibilityRequestData)x).Type == CheckEligibilityType.TwoYearOffer, () =>
            {
                RuleFor(x => ((CheckEligibilityRequestData)x).NationalInsuranceNumber)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage(ValidationMessages.RequiredNI)
                    .Must(DataValidation.BeAValidNi)
                    .WithMessage(ValidationMessages.InvalidNI);
            }).Otherwise(() =>
            {
                When(x => !string.IsNullOrEmpty(((CheckEligibilityRequestData)x).NationalInsuranceNumber), () =>
                {
                    RuleFor(x => ((CheckEligibilityRequestData)x).NationalAsylumSeekerServiceNumber)
                        .Empty()
                        .WithMessage(ValidationMessages.NI_and_NASS);

                    RuleFor(x => ((CheckEligibilityRequestData)x).NationalInsuranceNumber)
                        .Must(DataValidation.BeAValidNi)
                        .WithMessage(ValidationMessages.InvalidNI);

                }).Otherwise(() =>
                {
                    When(x => !string.IsNullOrEmpty(((CheckEligibilityRequestData)x).NationalAsylumSeekerServiceNumber), () =>
                    {
                        RuleFor(x => ((CheckEligibilityRequestData)x).NationalAsylumSeekerServiceNumber)
                            .Must(DataValidation.BeAValidNass)
                            .WithMessage(ValidationMessages.InvalidNASS);
                    });

                    RuleFor(x => ((CheckEligibilityRequestData)x).NationalAsylumSeekerServiceNumber)
                        .NotEmpty()
                        .WithMessage(ValidationMessages.NI_or_NASS);
                });
            });
        });

        When(x => x is CheckEligibilityRequestWorkingFamiliesData, () =>
        {
            RuleFor(x => ((CheckEligibilityRequestWorkingFamiliesData)x).EligibilityCode)
                .Cascade((CascadeMode.Stop))
                .NotEmpty().WithMessage(ValidationMessages.RequiredEligibilityCode)
                .Must(x => Regex.IsMatch(x.Trim(), @"^[+-]?\d+$")).WithMessage(ValidationMessages.EligibilityCodeNumber)
                .Must(x => x.Length == 11).WithMessage(ValidationMessages.EligibilityCodeIncorrectLength);
            RuleFor(x => ((CheckEligibilityRequestWorkingFamiliesData)x).NationalInsuranceNumber)
             .Cascade((CascadeMode.Stop))
             .NotEmpty().WithMessage(ValidationMessages.RequiredNI)
             .Must((x, NINumber) => string.IsNullOrEmpty(NINumber) || DataValidation.BeAValidNi(NINumber))
             .WithMessage(ValidationMessages.InvalidNI);

            RuleFor(x => ((CheckEligibilityRequestWorkingFamiliesData)x).DateOfBirth)
                .Cascade((CascadeMode.Stop))
                .NotEmpty().WithMessage(ValidationMessages.RequiredDOB)
                .Must((x, dob) => string.IsNullOrEmpty(dob) || DataValidation.BeAValidDate(dob))
                .WithMessage(ValidationMessages.InvalidDOB);

        });
    }
}