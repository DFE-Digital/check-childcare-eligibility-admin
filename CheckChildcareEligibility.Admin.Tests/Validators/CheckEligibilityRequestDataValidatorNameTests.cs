using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Validation;
using FluentAssertions;
using FluentValidation;

namespace CheckChildcareEligibility.Admin.Tests.Validators;

[TestFixture]
public class CheckEligibilityRequestDataValidatorNameTests
{
    private IValidator<IEligibilityServiceType> _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new CheckEligibilityRequestDataValidator();
    }

    [TestCase("García")]
    [TestCase("O'Connor")]
    [TestCase("O\u2019Connor")]
    [TestCase("Smith-Jones")]
    public void Validate_EyppSupportedLastName_Passes(string lastName)
    {
        var request = CreateValidRequest(
            CheckEligibilityType.EarlyYearPupilPremium,
            lastName);

        var result = _sut.Validate(request);

        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void Validate_TwoYearOfferAccentedLastName_Passes()
    {
        var request = CreateValidRequest(
            CheckEligibilityType.TwoYearOffer,
            "García");

        var result = _sut.Validate(request);

        result.Errors.Should().BeEmpty();
    }

    [TestCase(CheckEligibilityType.EarlyYearPupilPremium)]
    [TestCase(CheckEligibilityType.TwoYearOffer)]
    public void Validate_UnsupportedLastName_ReturnsInvalidCharacterMessage(
        CheckEligibilityType eligibilityType)
    {
        var request = CreateValidRequest(eligibilityType, "Smith@");

        var result = _sut.Validate(request);

        result.Errors
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal(ValidationMessages.ValidLastName);
    }

    [TestCase(CheckEligibilityType.EarlyYearPupilPremium)]
    [TestCase(CheckEligibilityType.TwoYearOffer)]
    public void Validate_MissingLastName_ReturnsOnlyRequiredMessage(
        CheckEligibilityType eligibilityType)
    {
        var request = CreateValidRequest(eligibilityType, string.Empty);

        var result = _sut.Validate(request);

        result.Errors
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal(ValidationMessages.RequiredLastName);
    }

    private static CheckEligibilityRequestData CreateValidRequest(
        CheckEligibilityType eligibilityType,
        string lastName)
    {
        return new CheckEligibilityRequestData
        {
            LastName = lastName,
            DateOfBirth = "1980-01-01",
            NationalInsuranceNumber = "AB124456A",
            Type = eligibilityType,
            Order = 1
        };
    }
}