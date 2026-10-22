using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Validation;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Validators;

[TestFixture]
public class FosterRequestValidatorNameTests
{
    [Test]
    public void FosterCarer_EmptyFirstName_ReturnsOnlyRequiredMessage()
    {
        var sut = new FosterCarerRequestValidator();

        var request = new FosterCarerRequest
        {
            CarerFirstName = string.Empty,
            CarerLastName = "Smith",
            CarerNationalInsuranceNumber = "AB123456C"
        };

        var result = sut.Validate(request);

        result.Errors
            .Where(x => x.PropertyName == nameof(FosterCarerRequest.CarerFirstName))
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal(FosterFamilyValidationMessages.CarerFirstNameEmpty);
    }

    [Test]
    public void FosterCarer_EmptyLastName_ReturnsOnlyRequiredMessage()
    {
        var sut = new FosterCarerRequestValidator();

        var request = new FosterCarerRequest
        {
            CarerFirstName = "John",
            CarerLastName = string.Empty,
            CarerNationalInsuranceNumber = "AB123456C"
        };

        var result = sut.Validate(request);

        result.Errors
            .Where(x => x.PropertyName == nameof(FosterCarerRequest.CarerLastName))
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal(FosterFamilyValidationMessages.CarerLastNameEmpty);
    }

    [Test]
    public void FosterPartner_EmptyFirstName_ReturnsOnlyRequiredMessage()
    {
        var sut = new FosterPartnerRequestValidator();

        var request = new FosterPartnerRequest
        {
            PartnerFirstName = string.Empty,
            PartnerLastName = "Smith",
            PartnerNationalInsuranceNumber = "AB123456C"
        };

        var result = sut.Validate(request);

        result.Errors
            .Where(x => x.PropertyName == nameof(FosterPartnerRequest.PartnerFirstName))
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal(FosterFamilyValidationMessages.PartnerFirstNameEmpty);
    }

    [Test]
    public void FosterPartner_EmptyLastName_ReturnsOnlyRequiredMessage()
    {
        var sut = new FosterPartnerRequestValidator();

        var request = new FosterPartnerRequest
        {
            PartnerFirstName = "Jane",
            PartnerLastName = string.Empty,
            PartnerNationalInsuranceNumber = "AB123456C"
        };

        var result = sut.Validate(request);

        result.Errors
            .Where(x => x.PropertyName == nameof(FosterPartnerRequest.PartnerLastName))
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal(FosterFamilyValidationMessages.PartnerLastNameEmpty);
    }

    [Test]
    public void FosterChild_EmptyFirstName_ReturnsOnlyRequiredMessage()
    {
        var sut = new FosterChildRequestValidator();

        var request = new FosterChildRequest
        {
            ChildFirstName = string.Empty,
            ChildLastName = "Smith",
            ChildPostCode = "M1 1AA",
            ChildDateOfBirth = DateTime.Today.AddYears(-3)
        };

        var result = sut.Validate(request);

        result.Errors
            .Where(x => x.PropertyName == nameof(FosterChildRequest.ChildFirstName))
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal(FosterFamilyValidationMessages.ChildFirstNameEmpty);
    }

    [Test]
    public void FosterChild_EmptyLastName_ReturnsOnlyRequiredMessage()
    {
        var sut = new FosterChildRequestValidator();

        var request = new FosterChildRequest
        {
            ChildFirstName = "Sam",
            ChildLastName = string.Empty,
            ChildPostCode = "M1 1AA",
            ChildDateOfBirth = DateTime.Today.AddYears(-3)
        };

        var result = sut.Validate(request);

        result.Errors
            .Where(x => x.PropertyName == nameof(FosterChildRequest.ChildLastName))
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal(FosterFamilyValidationMessages.ChildLastNameEmpty);
    }
}