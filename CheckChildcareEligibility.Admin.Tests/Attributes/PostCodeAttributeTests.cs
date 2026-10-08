using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.Attributes;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Attributes;

[TestFixture]
public class PostCodeAttributeTests
{
    [TestCase("SW1A 1AA")]
    [TestCase("SW1A1AA")]
    public void Validate_ValidUkPostcode_ReturnsSuccess(string postcode)
    {
        var model = new FosterChildDetailsViewModel
        {
            ChildPostCode = postcode
        };

        var attribute = new PostCodeAttribute();
        var context = new ValidationContext(model)
        {
            MemberName = nameof(FosterChildDetailsViewModel.ChildPostCode)
        };

        var result = attribute.GetValidationResult(postcode, context);

        result.Should().Be(ValidationResult.Success);
    }
}
