using System.Security.Claims;
using AutoFixture;
using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility.Admin.Tests.UseCases;

[TestFixture]
public class CreateUserUseCaseTests
{
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<CreateUserUseCase>>();
        _parentGatewayMock = new Mock<IParentGateway>();
        _sut = new CreateUserUseCase(_loggerMock.Object, _parentGatewayMock.Object);
        _fixture = new Fixture();
    }

    private CreateUserUseCase _sut;
    private Mock<ILogger<CreateUserUseCase>> _loggerMock;
    private Mock<IParentGateway> _parentGatewayMock;
    private Fixture _fixture;

    [Test]
    public async Task Execute_WithValidClaims_CreatesUser()
    {
        // Arrange
        var claims = CreateValidDfeClaims();
        var response = new UserSaveItemResponse { Data = "user123" };

        _parentGatewayMock
            .Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.Execute(claims);

        // Assert
        result.Should().Be(response.Data);
        _parentGatewayMock.Verify(x => x.CreateUser(It.IsAny<UserCreateRequest>()), Times.Once);
    }

    private IEnumerable<Claim> CreateValidDfeClaims()
    {
        // Ensure these claims include all required types that DfeSignInExtensions.GetDfeClaims expects.
        return new List<Claim>
        {
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "user123"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            // Additional claims that might be required by your DfE Sign-In logic:
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User")
        };
    }
}