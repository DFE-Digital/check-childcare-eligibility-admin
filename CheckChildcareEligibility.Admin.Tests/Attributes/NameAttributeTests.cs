using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.Attributes;
using CheckChildcareEligibility.Admin.Models;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Attributes;

[TestFixture]
public class NameAttributeTests
{
    private NameAttribute _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new NameAttribute();
    }

    [TestCase("Smith")]
    [TestCase("O'Brien")]         // straight apostrophe U+0027
    [TestCase("O\u2019Brien")]    // curly/right single quotation mark U+2019
    [TestCase("Smith-Jones")]
    [TestCase("St. Claire")]
    public void IsValid_ValidLastName_ReturnsSuccess(string lastName)
    {
        var model = new ParentGuardian { LastName = lastName };
        var context = new ValidationContext(model) { MemberName = "LastName" };

        var result = _sut.GetValidationResult(model.LastName, context);

        result.Should().BeNull();
    }

    [TestCase("Smith123")]
    [TestCase("O'Brien@")]
    [TestCase("Test!")]
    public void IsValid_InvalidLastName_ReturnsError(string lastName)
    {
        var model = new ParentGuardian { LastName = lastName };
        var context = new ValidationContext(model) { MemberName = "LastName" };

        var result = _sut.GetValidationResult(model.LastName, context);

        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Be("Enter a last name with valid characters");
    }
}

[TestFixture]
public class LastNameAttributeTests
{
    private LastNameAttribute _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new LastNameAttribute("Last name", "parent", null);
    }

    [TestCase("Smith")]
    [TestCase("O'Brien")]         // straight apostrophe U+0027
    [TestCase("O\u2019Brien")]    // curly/right single quotation mark U+2019
    [TestCase("Smith-Jones")]
    [TestCase("")]                // empty allowed (required handled separately)
    public void IsValid_ValidLastName_ReturnsSuccess(string lastName)
    {
        var context = new ValidationContext(new object());

        var result = _sut.GetValidationResult(lastName, context);

        result.Should().BeNull();
    }

    [TestCase("Smith123")]
    [TestCase("O'Brien@")]
    [TestCase("Test!")]
    public void IsValid_InvalidLastName_ReturnsError(string lastName)
    {
        var context = new ValidationContext(new object());

        var result = _sut.GetValidationResult(lastName, context);

        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Be("Last name contains an invalid character");
    }
}
