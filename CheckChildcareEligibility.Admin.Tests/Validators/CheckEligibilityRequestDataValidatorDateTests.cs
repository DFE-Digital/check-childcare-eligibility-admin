using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Validation;
using FluentAssertions;
using FluentValidation;

namespace CheckChildcareEligibility.Admin.Tests.Validators;

[TestFixture]
public class CheckEligibilityRequestDataValidatorDateTests
{
    private IValidator<IEligibilityServiceType> _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new CheckEligibilityRequestDataValidator();
    }

    [TestCase("1980-01-31")]
    [TestCase("31-01-1980")]
    public void Validate_SupportedDateFormat_Passes(string dateOfBirth)
    {
        var request = CreateValidRequest(dateOfBirth);

        var result = _sut.Validate(request);

        result.Errors.Should().BeEmpty();
    }

    [TestCase("1980/01/31")]
    [TestCase("31/01/1980")]
    [TestCase("31 Jan 1980")]
    public void Validate_UnsupportedDateFormat_ReturnsInvalidDateMessage(string dateOfBirth)
    {
        var request = CreateValidRequest(dateOfBirth);

        var result = _sut.Validate(request);

        result.Errors
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal(ValidationMessages.ValidDOB);
    }

    private static CheckEligibilityRequestData CreateValidRequest(string dateOfBirth)
    {
        return new CheckEligibilityRequestData
        {
            LastName = "Smith",
            DateOfBirth = dateOfBirth,
            NationalInsuranceNumber = "AB123456C",
            Type = CheckEligibilityType.EarlyYearPupilPremium,
            Order = 1
        };
    }
}
