using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ValidateFosterCodeReconfirmDateUseCaseTests
{
    private ValidateFosterCodeReconfirmDateUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ValidateFosterCodeReconfirmDateUseCase(
            new Mock<ILogger<ValidateFosterCodeReconfirmDateUseCase>>().Object);
    }

    [Test]
    public void Execute_WhenTodayIsSelected_UsesToday()
    {
        var request = new FosterCodeReconfirmDateViewModel { IsTodaySelected = true };

        var result = _sut.Execute(request, new ModelStateDictionary());

        result.IsValid.Should().BeTrue();
        request.SubmissionDate.Should().Be(DateTime.Today);
    }

    [Test]
    public void Execute_WhenAnotherDateIsSelected_UsesDateParts()
    {
        var request = new FosterCodeReconfirmDateViewModel
        {
            IsTodaySelected = false,
            Day = "12",
            Month = "4",
            Year = "2026"
        };

        var result = _sut.Execute(request, new ModelStateDictionary());

        result.IsValid.Should().BeTrue();
        request.SubmissionDate.Should().Be(new DateTime(2026, 4, 12));
    }

    [Test]
    public void Execute_WhenModelStateIsInvalid_ReturnsErrorsWithoutChangingDate()
    {
        var request = new FosterCodeReconfirmDateViewModel { SubmissionDate = new DateTime(2026, 1, 1) };
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("IsTodaySelected", "Select a date");

        var result = _sut.Execute(request, modelState);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("IsTodaySelected");
        request.SubmissionDate.Should().Be(new DateTime(2026, 1, 1));
    }
}
