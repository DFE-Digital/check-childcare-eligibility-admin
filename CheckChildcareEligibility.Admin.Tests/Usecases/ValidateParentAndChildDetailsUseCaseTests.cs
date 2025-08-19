using AutoFixture;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ValidateParentAndChildDetailsUseCaseTests
{
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ValidateParentAndChildDetailsUseCase>>();
        _sut = new ValidateParentAndChildDetailsUseCase(_loggerMock.Object);
        _fixture = new Fixture();
    }

    private Mock<ILogger<ValidateParentAndChildDetailsUseCase>> _loggerMock;
    private ValidateParentAndChildDetailsUseCase _sut;
    private Fixture _fixture;

    [Test]
    public void Execute_WhenModelStateValid_ShouldReturnValidResult()
    {
       // Arrange
       var request = _fixture.Create<ParentAndChildViewModel>();
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
        var request = _fixture.Create<ParentAndChildViewModel>();
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
        var request = _fixture.Create<ParentAndChildViewModel>();
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
        var request = _fixture.Create<ParentAndChildViewModel>();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("EligibilityCode", "Eligibility code is required");
        modelState.AddModelError("NationalInsuranceNumber", "Invalid format");
        modelState.AddModelError("ChildDateOfBirth", "Invalid date");

        // Act
        var result = _sut.Execute(request, modelState);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().ContainKey("EligibilityCode");
        result.Errors.Should().ContainKey("NationalInsuranceNumber");
        result.Errors.Should().ContainKey("ChildDateOfBirth");
    }
}