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
                    .WithMessage(FosterFamilyValidationMessages.PartnerIsRequired);

                RuleFor(x => x.Partner!)
                    .SetValidator(new FosterPartnerRequestValidator());
            });

            RuleFor(x => x.SubmissionDate)
                .Must(DataValidation.BeAPastDate)
                .WithMessage(FosterFamilyValidationMessages.DateMustBeInPast);

            RuleFor(x => x.SubmissionDate)
                .Must(DataValidation.BeWithin31Days)
                .WithMessage(string.Format(FosterFamilyValidationMessages.DateMustBeAfter, DateTime.Today.AddDays(-31)));
        }
    }

    public class FosterCarerRequestValidator : AbstractValidator<FosterCarerRequest>
    {
        public FosterCarerRequestValidator()
        {
            RuleFor(x => x.CarerFirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.CarerFirstNameEmpty)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.CarerFirstNameInvalid);

            RuleFor(x => x.CarerLastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.CarerLastNameEmpty)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.CarerLastNameInvalid);

            RuleFor(x => x.CarerNationalInsuranceNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.CarerNationalInsuranceNumberEmpty)
                .Must(DataValidation.BeAValidNi)
                .WithMessage("Enter a National Insurance number in the correct format");
        }
    }

    public class FosterPartnerRequestValidator
        : AbstractValidator<FosterPartnerRequest>
    {
        public FosterPartnerRequestValidator()
        {
            RuleFor(x => x.PartnerFirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.PartnerFirstNameEmpty)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.PartnerFirstNameInvalid);

            RuleFor(x => x.PartnerLastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.PartnerLastNameEmpty)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.PartnerLastNameInvalid);

            RuleFor(x => x.PartnerNationalInsuranceNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.PartnerNationalInsuranceNumberEmpty)
                .Must(DataValidation.BeAValidNi)
                .WithMessage("Enter a National Insurance number in the correct format");
        }
    }

    public class FosterChildRequestValidator
        : AbstractValidator<FosterChildRequest>
    {
        public FosterChildRequestValidator()
        {
            RuleFor(x => x.ChildFirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.ChildFirstNameEmpty)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.ChildFirstNameInvalid);

            RuleFor(x => x.ChildLastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(FosterFamilyValidationMessages.ChildLastNameEmpty)
                .Must(DataValidation.BeAValidName)
                .WithMessage(FosterFamilyValidationMessages.ChildLastNameInvalid);

            RuleFor(x => x.ChildPostCode)
                .Must(DataValidation.BeAValidUkPostcode)
                .WithMessage(FosterFamilyValidationMessages.ChildPostCodeInvalid);

            RuleFor(x => x.ChildDateOfBirth)
                .Must(DataValidation.BeAValidChildAge)
                .WithMessage(FosterFamilyValidationMessages.ChildIsTooOld);
        }
    }

    public class FosterChildReconfirmRequestValidator
        : AbstractValidator<FosterChildReconfirmRequest>
    {
        public FosterChildReconfirmRequestValidator()
        {
            RuleFor(x => x.SubmissionDate)
                .Must(DataValidation.BeAPastDate)
                .WithMessage(FosterFamilyValidationMessages.DateMustBeInPast);

            RuleFor(x => x.SubmissionDate)
                .Must(DataValidation.BeWithin31Days)
                .WithMessage(string.Format(FosterFamilyValidationMessages.DateMustBeAfter, DateTime.Today.AddDays(-31)));
        }
    }

}
