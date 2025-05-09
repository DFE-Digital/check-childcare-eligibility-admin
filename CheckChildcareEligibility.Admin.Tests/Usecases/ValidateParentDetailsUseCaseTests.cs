using AutoFixture;
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
       var result = _sut.Execute(request, modelState);

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
        var result = _sut.Execute(request, modelState);

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
        var result = _sut.Execute(request, modelState);

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
        var result = _sut.Execute(request, modelState);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().ContainKey("LastName");
        result.Errors.Should().ContainKey("NationalInsuranceNumber");
        result.Errors.Should().ContainKey("DateOfBirth");
    }
}