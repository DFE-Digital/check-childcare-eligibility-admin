using AutoFixture;
using CheckChildcareEligibility.Admin.Domain.Constants;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ValidateParentDetailsUseCaseTests
{
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ValidateParentDetailsUseCase>>();
        _sut = new ValidateParentDetailsUseCase(_loggerMock.Object);
        _fixture = new Fixture();
    }

    private Mock<ILogger<ValidateParentDetailsUseCase>> _loggerMock;
    private ValidateParentDetailsUseCase _sut;
    private Fixture _fixture;

    [Test]
    public void Execute_WhenModelStateValid_ShouldReturnValidResult()
    {
       // Arrange
       var request = _fixture.Create<ParentGuardian>();
       var modelState = new ModelStateDictionary();

       // Act
       var result = _sut.Execute(request, modelState, "2YO");

       // Assert
       result.IsValid.Should().BeTrue();
       result.Errors.Should().BeNull();
    }

    [Test]
    public void Execute_WhenModelStateInvalid_ShouldReturnInvalidResult()
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("TestKey", "Test Error");

        // Act
        var result = _sut.Execute(request, modelState, "2YO");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().ContainKey("TestKey");
    }

    [Test]
    public void Execute_WhenNinValidationFails_ShouldReturnInvalidResult()
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("NationalInsuranceNumber", "Invalid format");

        // Act
        var result = _sut.Execute(request, modelState, "2YO");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().ContainKey("NationalInsuranceNumber");
    }

    [Test]
    public void Execute_WhenMultipleFieldsHaveErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("LastName", "Last name is required");
        modelState.AddModelError("NationalInsuranceNumber", "Invalid format");
        modelState.AddModelError("DateOfBirth", "Invalid date");

        // Act
        var result = _sut.Execute(request, modelState, "2YO");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().ContainKey("LastName");
        result.Errors.Should().ContainKey("NationalInsuranceNumber");
        result.Errors.Should().ContainKey("DateOfBirth");
    }

    [TestCase("", "", ValidationMessages.NI_or_NASS)]
    [TestCase("invalid", "", "Enter a National Insurance number in the correct format")]
    [TestCase("", "12345", ValidationMessages.InvalidNASS)]
    public void Execute_WhenEyppIdentifiersAreInvalid_ReturnsExpectedError(
        string nino,
        string nass,
        string expectedError)
    {
        var request = new ParentGuardian
        {
            NationalInsuranceNumber = nino,
            NationalAsylumSeekerServiceNumber = nass
        };

        var result = _sut.Execute(request, new ModelStateDictionary(), "EYPP");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Value.Should().ContainSingle().Which.Should().Be(expectedError);
    }

    [TestCase("AB123456C", "")]
    [TestCase("", "123456789")]
    [TestCase("", "12/34/56789")]
    public void Execute_WhenEyppHasOneValidIdentifier_ReturnsValidResult(string nino, string nass)
    {
        var request = new ParentGuardian
        {
            NationalInsuranceNumber = nino,
            NationalAsylumSeekerServiceNumber = nass
        };

        var result = _sut.Execute(request, new ModelStateDictionary(), "EYPP");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Test]
    public void Execute_WhenEyppHasBothIdentifiers_DoesNotReturnMutualExclusionError()
    {
        var request = new ParentGuardian
        {
            NationalInsuranceNumber = "AB123456C",
            NationalAsylumSeekerServiceNumber = "123456789",
            IdentifierType = IdentifierTypeConstants.Nino
        };

        var result = _sut.Execute(request, new ModelStateDictionary(), "EYPP");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Test]
    public void Execute_WhenNotEypp_IgnoresNassValidationErrors()
    {
        var request = new ParentGuardian { NationalInsuranceNumber = "AB123456C" };
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("NationalAsylumSeekerServiceNumber", ValidationMessages.InvalidNASS);

        var result = _sut.Execute(request, modelState, "2YO");

        result.IsValid.Should().BeTrue();
    }
}