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
    [SetUp]
    public void SetUp()
    {
        _checkGatewayMock = new Mock<ICheckGateway>();
        _sut = new Perform2YoEligibilityCheckUseCase(_checkGatewayMock.Object);

        _sessionMock = new Mock<ISession>();
        var sessionStorage = new Dictionary<string, byte[]>();

        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

        _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                var result = sessionStorage.TryGetValue(key, out var storedValue);
                value = storedValue;
                return result;
            });

        _parent = new ParentGuardian
        {
            FirstName = "John",
            LastName = "Doe",
            Day = "01",
            Month = "01",
            Year = "1980",
            NationalInsuranceNumber = "AB123456C",
            EmailAddress = "a@b.c"
        };

        _eligibilityResponse = new CheckEligibilityResponse
        {
            Data = new StatusValue { Status = "queuedForProcessing" },
            Links = new CheckEligibilityResponseLinks { Get_EligibilityCheck = "test-link" }
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

        Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentFirstName")).Should().Be("John");
        Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentLastName")).Should().Be("Doe");
        Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentDOB")).Should().Be("1980-01-01");
        Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentNINO")).Should().Be("AB123456C");
    }

    [Test]
    public async Task Execute_WithNassParent_ShouldSetNassSessionData()
    {
        // Arrange
        _parent.NationalAsylumSeekerServiceNumber = "NASS123456";
        _parent.NinAsrSelection = ParentGuardian.NinAsrSelect.AsrnSelected;
        _checkGatewayMock.Setup(s => s.PostCheck(It.IsAny<CheckEligibilityRequest>()))
            .ReturnsAsync(_eligibilityResponse);

        // Act
        var response = await _sut.Execute(_parent, _sessionMock.Object);

        // Assert
        response.Should().BeEquivalentTo(_eligibilityResponse);

        Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentFirstName")).Should().Be("John");
        Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentLastName")).Should().Be("Doe");
        Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentDOB")).Should().Be("1980-01-01");
        Encoding.UTF8.GetString(_sessionMock.Object.Get("ParentNASS")).Should().Be("NASS123456");
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