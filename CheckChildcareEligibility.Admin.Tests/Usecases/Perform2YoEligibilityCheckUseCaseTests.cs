using System.Text;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class Perform2YoEligibilityCheckUseCaseTests
{
    private Dictionary<string, byte[]> _sessionStorage;

    [SetUp]
    public void SetUp()
    {
        _checkGatewayMock = new Mock<ICheckGateway>();
        _sut = new Perform2YoEligibilityCheckUseCase(_checkGatewayMock.Object);

        _sessionMock = new Mock<ISession>();
        _sessionStorage = new Dictionary<string, byte[]>();

        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => _sessionStorage[key] = value);

        _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                var result = _sessionStorage.TryGetValue(key, out var bytes);
                value = bytes;
                return result;
            });

        _parent = new ParentGuardian
        {
            LastName = "Doe",
            Day = "01",
            Month = "01",
            Year = "1980",
            NationalInsuranceNumber = "AB123456C"
        };

        _eligibilityResponse = new CheckEligibilityResponse
        {
            Data = new StatusValue { Status = "eligible" },
            Links = new CheckEligibilityResponseLinks()
        };
    }

    private Perform2YoEligibilityCheckUseCase _sut;
    private Mock<ICheckGateway> _checkGatewayMock;
    private Mock<ISession> _sessionMock;

    private ParentGuardian _parent;
    private CheckEligibilityResponse _eligibilityResponse;

    [Test]
    public async Task Execute_WithValidParent_ShouldReturnValidResponse()
    {
        // Arrange
        _checkGatewayMock.Setup(s => s.PostCheck(It.IsAny<CheckEligibilityRequest>()))
            .ReturnsAsync(_eligibilityResponse);

        // Act
        var response = await _sut.Execute(_parent, _sessionMock.Object);

        // Assert
        response.Should().BeEquivalentTo(_eligibilityResponse);

        // Verify that session has the expected values
        byte[] lastNameBytes;
        byte[] dobBytes;
        byte[] ninoBytes;
        
        _sessionMock.Object.TryGetValue("ParentLastName", out lastNameBytes);
        _sessionMock.Object.TryGetValue("ParentDOB", out dobBytes);
        _sessionMock.Object.TryGetValue("ParentNINO", out ninoBytes);
        
        Encoding.UTF8.GetString(lastNameBytes).Should().Be("Doe");
        Encoding.UTF8.GetString(dobBytes).Should().Be("1980-01-01");
        Encoding.UTF8.GetString(ninoBytes).Should().Be("AB123456C");
    }

    [TestCase("ab123456c")]
    public async Task Execute_Should_Canonicalise_Nino_Without_Changing_Submitted_Model(
    string input)
    {
        _parent.NationalInsuranceNumber = input;
        _sessionStorage.Clear();

        CheckEligibilityRequest? capturedRequest = null;

        _checkGatewayMock
            .Setup(g => g.PostCheck(It.IsAny<CheckEligibilityRequest>()))
            .Callback<CheckEligibilityRequest>(request => capturedRequest = request)
            .ReturnsAsync(_eligibilityResponse);

        await _sut.Execute(_parent, _sessionMock.Object);

        capturedRequest.Should().NotBeNull();
        var data = capturedRequest!.Data.Should()
            .BeOfType<CheckEligibilityRequestData>().Subject;

        data.Type.Should().Be(CheckEligibilityType.TwoYearOffer);
        data.NationalInsuranceNumber.Should().Be("AB123456C");

        _sessionStorage.Should().ContainKey("ParentNINO");
        Encoding.UTF8.GetString(_sessionStorage["ParentNINO"])
            .Should().Be("AB123456C");

        _parent.NationalInsuranceNumber.Should().Be(input);

        _checkGatewayMock.Verify(
            g => g.PostCheck(It.IsAny<CheckEligibilityRequest>()),
            Times.Once);
    }

    [Test]
    public async Task Execute_WhenApiThrowsException_ShouldThrow()
    {
        // Arrange
        _checkGatewayMock.Setup(s => s.PostCheck(It.IsAny<CheckEligibilityRequest>()))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        Func<Task> act = async () => await _sut.Execute(_parent, _sessionMock.Object);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("API Error");
    }
}