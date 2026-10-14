using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.Domain.Constants;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Attributes;

[TestFixture]
public class SubmissionDateAttributeTests
{
    [Test]
    public void Validate_DateExactlySubmissionDateWindowDaysAgo_ReturnsSuccess()
    {
        var submissionDate = DateTime.Today.AddDays(-FosterFamiliesConstants.SubmissionDateWindowDays);

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
    public void Validate_DateOneDayBeforeSubmissionDateWindow_ReturnsError()
    {
        var submissionDate = DateTime.Today.AddDays(-FosterFamiliesConstants.SubmissionDateWindowDays - 1);

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
