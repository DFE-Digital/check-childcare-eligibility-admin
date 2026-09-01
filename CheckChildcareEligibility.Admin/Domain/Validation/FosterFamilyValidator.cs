using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.API.Domain.Validation;
using FluentValidation;

namespace CheckChildcareEligibility.Admin.Domain.Validation
{
    public class FosterFamilyRequestValidator : AbstractValidator<FosterFamilyRequest>
    {
        public FosterFamilyRequestValidator()
        {
            RuleFor(x => x.FosterCarer)
                .NotNull();

            RuleFor(x => x.FosterChild)
                .NotNull();

            RuleFor(x => x.FosterCarer!)
                .SetValidator(new FosterCarerRequestValidator());

            RuleFor(x => x.FosterChild!)
                .SetValidator(new FosterChildRequestValidator());

            When(x => x.HasPartner, () =>
            {
                RuleFor(x => x.Partner)
                    .NotNull()
                    .WithMessage("Partner details are required.");

                RuleFor(x => x.Partner!)
                    .SetValidator(new FosterPartnerRequestValidator());
            });
        }
    }

    public class FosterCarerRequestValidator : AbstractValidator<FosterCarerRequest>
    {
        public FosterCarerRequestValidator()
        {
            RuleFor(x => x.CarerFirstName)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.CarerFirstNameEmpty);

            RuleFor(x => x.CarerFirstName)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.CarerFirstNameInvalid);

            RuleFor(x => x.CarerLastName)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.CarerLastNameEmpty);

            RuleFor(x => x.CarerLastName)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.CarerLastNameInvalid);
        }
    }

    public class FosterPartnerRequestValidator
        : AbstractValidator<FosterPartnerRequest>
    {
        public FosterPartnerRequestValidator()
        {
            RuleFor(x => x.PartnerFirstName)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.PartnerFirstNameEmpty);

            RuleFor(x => x.PartnerFirstName)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.PartnerFirstNameInvalid);

            RuleFor(x => x.PartnerLastName)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.PartnerLastNameEmpty);

            RuleFor(x => x.PartnerLastName)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.PartnerLastNameInvalid);
        }
    }

    public class FosterChildRequestValidator
        : AbstractValidator<FosterChildRequest>
    {
        public FosterChildRequestValidator()
        {
            RuleFor(x => x.ChildFirstName)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.ChildFirstNameEmpty);

            RuleFor(x => x.ChildFirstName)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.ChildFirstNameInvalid);

            RuleFor(x => x.ChildLastName)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.ChildLastNameEmpty);

            RuleFor(x => x.ChildLastName)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.ChildLastNameInvalid);

            RuleFor(x => x.ChildPostCode)
                .Must(DataValidation.BeAValidUkPostcode)
                .WithMessage(FosterFamilyValidationMessages.ChildPostCodeInvalid);
        }
    }
}
