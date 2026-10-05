using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ValidateFosterChildDetailsUseCaseTests
{
    private ValidateFosterChildDetailsUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ValidateFosterChildDetailsUseCase(
            new Mock<ILogger<ValidateFosterChildDetailsUseCase>>().Object);
    }

    [Test]
    public void Execute_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        FluentActions.Invoking(() => _sut.Execute(null!, new ModelStateDictionary()))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Execute_WhenModelStateIsInvalid_ReturnsErrorsWithoutConstructingDate()
    {
        var request = new FosterChildDetailsViewModel { Day = "invalid" };
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("ChildFirstName", "Child first name is required");

        var result = _sut.Execute(request, modelState);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("ChildFirstName");
        request.ChildDateOfBirth.Should().Be(default(DateTime));
    }

    [Test]
    public void Execute_WhenRequestIsValid_ConstructsDateAndReturnsValid()
    {
        var request = new FosterChildDetailsViewModel
        {
            ChildFirstName = "Child",
            ChildLastName = "Smith",
            ChildPostCode = "SW1A 1AA",
            Day = "12",
            Month = "4",
            Year = DateTime.Today.AddYears(-4).Year.ToString()
        };

        var result = _sut.Execute(request, new ModelStateDictionary());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeNull();
        request.ChildDateOfBirth.Should().Be(new DateTime(
            DateTime.Today.AddYears(-4).Year,
            4,
            12));
    }
}
