using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Validation;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Validators;

[TestFixture]
public class CheckEligibilityRequestDataValidatorIdentifierTests
{
    private CheckEligibilityRequestDataValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new CheckEligibilityRequestDataValidator();
    }

    [TestCase("AB123456C", "")]
    [TestCase("", "123456789")]
    public void Validate_Eypp_WithOneValidIdentifier_Passes(
        string nationalInsuranceNumber,
        string asylumSupportReference)
    {
        var request = CreateRequest(
            CheckEligibilityType.EarlyYearPupilPremium,
            nationalInsuranceNumber,
            asylumSupportReference);

        var result = _sut.Validate(request);

        result.Errors.Should().BeEmpty();
    }

    [TestCase("", "", ValidationMessages.NI_or_NASS)]
    [TestCase("AB123456C", "123456789", ValidationMessages.NI_and_NASS)]
    [TestCase("invalid", "", ValidationMessages.InvalidNI)]
    [TestCase("", "12345", ValidationMessages.InvalidNASS)]
    public void Validate_Eypp_WithInvalidIdentifierCombination_ReturnsExpectedError(
        string nationalInsuranceNumber,
        string asylumSupportReference,
        string expectedError)
    {
        var request = CreateRequest(
            CheckEligibilityType.EarlyYearPupilPremium,
            nationalInsuranceNumber,
            asylumSupportReference);

        var result = _sut.Validate(request);

        result.Errors.Select(error => error.ErrorMessage).Should().Equal(expectedError);
    }

    [Test]
    public void Validate_TwoYearOffer_WithValidNationalInsuranceNumber_Passes()
    {
        var request = CreateRequest(CheckEligibilityType.TwoYearOffer, "AB123456C");

        var result = _sut.Validate(request);

        result.Errors.Should().BeEmpty();
    }

    [TestCase("", ValidationMessages.RequiredNI)]
    [TestCase("invalid", ValidationMessages.InvalidNI)]
    public void Validate_TwoYearOffer_WithInvalidNationalInsuranceNumber_ReturnsExpectedError(
        string nationalInsuranceNumber,
        string expectedError)
    {
        var request = CreateRequest(CheckEligibilityType.TwoYearOffer, nationalInsuranceNumber);

        var result = _sut.Validate(request);

        result.Errors.Select(error => error.ErrorMessage).Should().Equal(expectedError);
    }

    [Test]
    public void Validate_WorkingFamilies_WithValidEligibilityCode_Passes()
    {
        var request = CreateWorkingFamiliesRequest();

        var result = _sut.Validate(request);

        result.Errors.Should().BeEmpty();
    }

    [TestCase("", "1980-01-01", ValidationMessages.RequiredNI)]
    [TestCase("AB123456C", "", ValidationMessages.RequiredDOB)]
    [TestCase("invalid", "1980-01-01", ValidationMessages.InvalidNI)]
    [TestCase("AB123456C", "not-a-date", ValidationMessages.InvalidDOB)]
    public void Validate_WorkingFamilies_WithInvalidRequiredData_ReturnsExpectedError(
        string nationalInsuranceNumber,
        string dateOfBirth,
        string expectedError)
    {
        var request = CreateWorkingFamiliesRequest(
            nationalInsuranceNumber: nationalInsuranceNumber,
            dateOfBirth: dateOfBirth);

        var result = _sut.Validate(request);

        result.Errors.Select(error => error.ErrorMessage).Should().Equal(expectedError);
    }

    [TestCase("", ValidationMessages.RequiredEligibilityCode)]
    [TestCase("12345A78901", ValidationMessages.EligibilityCodeNumber)]
    [TestCase("1234567890", ValidationMessages.EligibilityCodeIncorrectLength)]
    public void Validate_WorkingFamilies_WithInvalidEligibilityCode_ReturnsExpectedError(
        string eligibilityCode,
        string expectedError)
    {
        var request = CreateWorkingFamiliesRequest(eligibilityCode: eligibilityCode);

        var result = _sut.Validate(request);

        result.Errors
            .Where(error => error.PropertyName == nameof(CheckEligibilityRequestWorkingFamiliesData.EligibilityCode))
            .Select(error => error.ErrorMessage)
            .Should()
            .Equal(expectedError);
    }

    private static CheckEligibilityRequestWorkingFamiliesData CreateWorkingFamiliesRequest(
        string eligibilityCode = "12345678901",
        string nationalInsuranceNumber = "AB123456C",
        string dateOfBirth = "1980-01-01")
    {
        return new CheckEligibilityRequestWorkingFamiliesData
        {
            EligibilityCode = eligibilityCode,
            NationalInsuranceNumber = nationalInsuranceNumber,
            DateOfBirth = dateOfBirth
        };
    }

    private static CheckEligibilityRequestData CreateRequest(
        CheckEligibilityType eligibilityType,
        string nationalInsuranceNumber,
        string asylumSupportReference = "")
    {
        return new CheckEligibilityRequestData
        {
            LastName = "Smith",
            DateOfBirth = "1980-01-01",
            NationalInsuranceNumber = nationalInsuranceNumber,
            NationalAsylumSeekerServiceNumber = asylumSupportReference,
            Type = eligibilityType
        };
    }
}
