using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Attributes;

[TestFixture]
public class SubmissionDateAttributeTests
{
    [Test]
    public void Validate_DateExactly31DaysAgo_ReturnsSuccess()
    {
        var submissionDate = DateTime.Today.AddDays(-31);

        var model = new FosterApplicationSubmittedDateViewModel
        {
            IsTodaySelected = false,
            Day = submissionDate.Day.ToString(),
            Month = submissionDate.Month.ToString(),
            Year = submissionDate.Year.ToString()
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Test]
    public void Validate_Date32DaysAgo_ReturnsError()
    {
        var submissionDate = DateTime.Today.AddDays(-32);

        var model = new FosterApplicationSubmittedDateViewModel
        {
            IsTodaySelected = false,
            Day = submissionDate.Day.ToString(),
            Month = submissionDate.Month.ToString(),
            Year = submissionDate.Year.ToString()
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);

        isValid.Should().BeFalse();
        results.Should().ContainSingle();
    }
}
