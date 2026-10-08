using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ValidateFosterApplicationSubmittedDateUseCaseTests
{
    private ValidateFosterApplicationSubmittedDateUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ValidateFosterApplicationSubmittedDateUseCase(
            new Mock<ILogger<ValidateFosterApplicationSubmittedDateUseCase>>().Object);
    }

    [Test]
    public async Task Execute_WhenTodayIsSelected_UsesToday()
    {
        var request = new FosterApplicationSubmittedDateViewModel { IsTodaySelected = true };

        var result = await _sut.Execute(request, new ModelStateDictionary());

        result.IsValid.Should().BeTrue();
        request.SubmissionDate.Should().Be(DateTime.Today);
    }

    [Test]
    public async Task Execute_WhenAnotherDateIsSelected_UsesDateParts()
    {
        var request = new FosterApplicationSubmittedDateViewModel
        {
            IsTodaySelected = false,
            Day = "12",
            Month = "4",
            Year = "2026"
        };

        var result = await _sut.Execute(request, new ModelStateDictionary());

        result.IsValid.Should().BeTrue();
        request.SubmissionDate.Should().Be(new DateTime(2026, 4, 12));
    }

    [Test]
    public async Task Execute_WhenModelStateIsInvalid_ReturnsErrorsWithoutChangingDate()
    {
        var request = new FosterApplicationSubmittedDateViewModel { SubmissionDate = new DateTime(2026, 1, 1) };
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("IsTodaySelected", "Select a date");

        var result = await _sut.Execute(request, modelState);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("IsTodaySelected");
        request.SubmissionDate.Should().Be(new DateTime(2026, 1, 1));
    }
}
