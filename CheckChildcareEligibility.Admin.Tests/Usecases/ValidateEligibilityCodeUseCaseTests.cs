using AutoFixture;
using CheckChildcareEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ValidateEligibilityCodeUseCaseTests
{
    private Mock<ILogger<ValidateEligibilityCodeUseCase>> _loggerMock;
    private ValidateEligibilityCodeUseCase _sut;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ValidateEligibilityCodeUseCase>>();
        _sut = new ValidateEligibilityCodeUseCase(_loggerMock.Object);
        _fixture = new Fixture();
    }
    [TestCase("", "Enter an eligibility code that is 11 digits long")]
    [TestCase("12345ABC678", "Eligibility code must only contain numbers")]
    [TestCase("1234567890", "Eligibility code must be 11 digits long")]
    public void Execute_WhenValidationFails_ShouldReturnExpectedErrorMessage(string eligibilityCode, string expectedError)
    {
        var result = _sut.Execute(eligibilityCode);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("EligibilityCode");
        result.Errors["EligibilityCode"].Should().Contain(expectedError);
    }

    [Test]
    public void Execute_WhenValidationPasses_ShouldReturnNull()
    {
        var result = _sut.Execute("12345678912");
        result.Errors.Should().BeNullOrEmpty();
    }
}
