using System.ComponentModel.DataAnnotations;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.Tests.Attributes.Derived;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Attributes;

public class NinoAttributeTests
{
    private const string NINOMissingErrorMessage = "Enter a National Insurance number";
    private const string NINOFormatErrorMessage = "Enter a National Insurance number in the correct format";

    private TestableNinoAttribute _ninoAttribute { get; set; }
    private ValidationContext _validationContext { get; set; }

    [SetUp]
    public void Setup()
    {
        _ninoAttribute = new TestableNinoAttribute();
        var parentGuardian = new ParentGuardian();
        _validationContext = new ValidationContext(parentGuardian)
        {
            DisplayName = nameof(parentGuardian.NationalInsuranceNumber)
        };
    }

    [TestCase(null, NINOMissingErrorMessage)]
    [TestCase("DB123456C", NINOFormatErrorMessage)]
    [TestCase("FB123456C", NINOFormatErrorMessage)]
    [TestCase("IB123456C", NINOFormatErrorMessage)]
    [TestCase("QB123456C", NINOFormatErrorMessage)]
    [TestCase("UB123456C", NINOFormatErrorMessage)]
    [TestCase("VB123456C", NINOFormatErrorMessage)]
    [TestCase("AD123456C", NINOFormatErrorMessage)]
    [TestCase("AF123456C", NINOFormatErrorMessage)]
    [TestCase("AI123456C", NINOFormatErrorMessage)]
    [TestCase("AQ123456C", NINOFormatErrorMessage)]
    [TestCase("AU123456C", NINOFormatErrorMessage)]
    [TestCase("AV123456C", NINOFormatErrorMessage)]
    [TestCase("AO123456C", NINOFormatErrorMessage)]
    [TestCase("BG123456C", NINOFormatErrorMessage)]
    [TestCase("GB123456C", NINOFormatErrorMessage)]
    [TestCase("KN123456C", NINOFormatErrorMessage)]
    [TestCase("NK123456C", NINOFormatErrorMessage)]
    [TestCase("NT123456C", NINOFormatErrorMessage)]
    [TestCase("TN123456C", NINOFormatErrorMessage)]
    [TestCase("ZZ123456C", NINOFormatErrorMessage)]
    [TestCase("ZZ123456C", NINOFormatErrorMessage)]
    [TestCase("AB123456E", NINOFormatErrorMessage)]
    [TestCase("AB\u0661\u0662\u0663\u0664\u0665\u0666C", NINOFormatErrorMessage)]
    [TestCase("AB\uFF11\uFF12\uFF13\uFF14\uFF15\uFF16C", NINOFormatErrorMessage)]
    [TestCase("---", NINOFormatErrorMessage)]
    public void Given_Nino_When_Invalid_Should_ReturnErrorMessage(string? nino, string? errorMessage)
    {
        // Act
        var result = _ninoAttribute.NinoIsValid(nino, _validationContext);

        // Assert
        Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [TestCase("ab123456c")]
    [TestCase("AB123456A")]
    [TestCase("AB123456C")]
    [TestCase("AB123456B")]
    [TestCase("AB123456C")]
    [TestCase("AB123456D")]
    [TestCase("ab 12 34 56 c")]
    [TestCase("ab-12.34/56c")]
    [TestCase("ab\t12\r\n3456c")]
    public void Given_Nino_When_Valid_Should_ReturnNull(string? nino)
    {
        // Act
        var result = _ninoAttribute.NinoIsValid(nino, _validationContext);

        // Assert
        result.Should().BeNull(nino);
    }

    [TestCase("ab 12 34 56 c")]
    [TestCase("ab-12.34/56c")]
    [TestCase("ab\t12\r\n3456c")]
    public void Given_Valid_Nino_Should_Not_Change_Submitted_Model(string input)
    {
        var parent = new ParentGuardian
        {
            NationalInsuranceNumber = input
        };

        var context = new ValidationContext(parent)
        {
            MemberName = nameof(ParentGuardian.NationalInsuranceNumber),
            DisplayName = "National Insurance number"
        };

        var result = _ninoAttribute.NinoIsValid(input, context);

        result.Should().BeNull();
        parent.NationalInsuranceNumber.Should().Be(input);
    }

    [TestCase("ab 12 34 56 c")]
    [TestCase("ab-12.34/56c")]
    [TestCase("ab\t12\r\n3456c")]
    public void Given_Valid_Nino_Model_Attributes_Should_Accept_Without_Mutation(
    string input)
    {
        var parent = new ParentGuardian
        {
            NationalInsuranceNumber = input
        };

        var context = new ValidationContext(parent)
        {
            MemberName = nameof(ParentGuardian.NationalInsuranceNumber)
        };
        var errors = new List<ValidationResult>();

        var valid = Validator.TryValidateProperty(input, context, errors);

        valid.Should().BeTrue();
        errors.Should().BeEmpty();
        parent.NationalInsuranceNumber.Should().Be(input);
    }

    [TestCase(false, null)]
    [TestCase(false, "")]
    [TestCase(false, "   ")]
    [TestCase(true, null)]
    [TestCase(true, "")]
    [TestCase(true, "   ")]
    public void Given_Missing_Foster_Nino_Should_Return_Required_Message(
        bool partner,
        string? input)
    {
        object model;
        string propertyName;
        string expectedMessage;

        if (partner)
        {
            model = new FosterPartnerDetailsViewModel
            {
                PartnerNationalInsuranceNumber = input!
            };
            propertyName = nameof(
                FosterPartnerDetailsViewModel.PartnerNationalInsuranceNumber);
            expectedMessage =
                FosterFamilyValidationMessages.PartnerNationalInsuranceNumberEmpty;
        }
        else
        {
            model = new FosterCarerDetailsViewModel
            {
                CarerNationalInsuranceNumber = input!
            };
            propertyName = nameof(
                FosterCarerDetailsViewModel.CarerNationalInsuranceNumber);
            expectedMessage =
                FosterFamilyValidationMessages.CarerNationalInsuranceNumberEmpty;
        }

        var context = new ValidationContext(model)
        {
            MemberName = propertyName
        };
        var errors = new List<ValidationResult>();

        var valid = Validator.TryValidateProperty(input, context, errors);

        valid.Should().BeFalse();
        errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(expectedMessage);
    }
}