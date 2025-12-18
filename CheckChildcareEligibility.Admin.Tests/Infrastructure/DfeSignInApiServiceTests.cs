using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using CheckChildcareEligibility.Admin.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.Infrastructure;

/// <summary>
/// Test handler that captures HTTP requests for verification.
/// </summary>
internal class TestHttpMessageHandler : HttpMessageHandler
{
    public HttpRequestMessage? CapturedRequest { get; private set; }
    public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }
    public Exception? ExceptionToThrow { get; set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CapturedRequest = request;
        
        if (ExceptionToThrow != null)
        {
            throw ExceptionToThrow;
        }
        
        var response = ResponseFactory?.Invoke(request) ?? new HttpResponseMessage(HttpStatusCode.OK);
        return Task.FromResult(response);
    }
}

[TestFixture]
internal class DfeSignInApiServiceTests
{
    private Mock<IDfeSignInConfiguration> _mockConfiguration;
    private Mock<ILogger<DfeSignInApiService>> _mockLogger;

    private const string TestClientId = "TestClientId";
    private const string TestApiSecret = "test-api-secret-key-that-is-long-enough-for-hmac-sha256";
    private const string TestApiProxyUrl = "https://test-api.signin.education.gov.uk";

    [SetUp]
    public void SetUp()
    {
        _mockConfiguration = new Mock<IDfeSignInConfiguration>();
        _mockLogger = new Mock<ILogger<DfeSignInApiService>>();

        _mockConfiguration.Setup(x => x.ClientId).Returns(TestClientId);
        _mockConfiguration.Setup(x => x.APIServiceSecret).Returns(TestApiSecret);
        _mockConfiguration.Setup(x => x.APIServiceProxyUrl).Returns(TestApiProxyUrl);
    }

    private (DfeSignInApiService sut, TestHttpMessageHandler handler, HttpClient httpClient) CreateSutWithTestHandler()
    {
        var handler = new TestHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var sut = new DfeSignInApiService(httpClient, _mockConfiguration.Object, _mockLogger.Object);
        return (sut, handler, httpClient);
    }

    [Test]
    public async Task GetUserRolesAsync_WithValidResponse_ReturnsRoles()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var responseJson = $$"""
            {
                "userId": "{{userId}}",
                "organisationId": "{{organisationId}}",
                "serviceId": "{{TestClientId}}",
                "roles": [
                    {
                        "id": "{{roleId}}",
                        "code": "mefcsLocalAuthority",
                        "name": "MEFCS - Local Authority Role",
                        "numericId": "22440"
                    }
                ]
            }
            """;

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        // Act
        var result = await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Code.Should().Be("mefcsLocalAuthority");
        result.First().Name.Should().Be("MEFCS - Local Authority Role");

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_WithMultipleRoles_ReturnsAllRoles()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();

        // Use raw JSON to match the expected DfE Sign-in API response format
        var responseJson = $$"""
            {
                "userId": "{{userId}}",
                "organisationId": "{{organisationId}}",
                "serviceId": "{{TestClientId}}",
                "roles": [
                    {
                        "id": "{{roleId1}}",
                        "code": "mefcsLocalAuthority",
                        "name": "MEFCS - Local Authority Role",
                        "numericId": "22440"
                    },
                    {
                        "id": "{{roleId2}}",
                        "code": "anotherRole",
                        "name": "Another Role",
                        "numericId": "22441"
                    }
                ]
            }
            """;

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        // Act
        var result = await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        result.Should().HaveCount(2);

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_WithEmptyRoles_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        var responseJson = $$"""
            {
                "userId": "{{userId}}",
                "organisationId": "{{organisationId}}",
                "serviceId": "{{TestClientId}}",
                "roles": []
            }
            """;

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        // Act
        var result = await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        result.Should().BeEmpty();

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_WithForbiddenResponse_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("{\"success\":false,\"message\":\"invalid signature\"}", Encoding.UTF8, "application/json")
        };

        // Act
        var result = await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        result.Should().BeEmpty();

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_WithNotFoundResponse_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("{\"success\":false,\"message\":\"user not found\"}", Encoding.UTF8, "application/json")
        };

        // Act
        var result = await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        result.Should().BeEmpty();

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_WithMissingApiProxyUrl_ReturnsEmptyList()
    {
        // Arrange
        _mockConfiguration.Setup(x => x.APIServiceProxyUrl).Returns(string.Empty);
        var (sut, _, httpClient) = CreateSutWithTestHandler();
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        // Act
        var result = await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        result.Should().BeEmpty();

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_WithMissingApiSecret_ReturnsEmptyList()
    {
        // Arrange
        _mockConfiguration.Setup(x => x.APIServiceSecret).Returns(string.Empty);
        var (sut, _, httpClient) = CreateSutWithTestHandler();
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        // Act
        var result = await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        result.Should().BeEmpty();

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_WithHttpException_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ExceptionToThrow = new HttpRequestException("Network error");

        // Act
        var result = await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        result.Should().BeEmpty();

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_CallsCorrectApiEndpoint()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        var responseJson = $$"""
            {
                "userId": "{{userId}}",
                "organisationId": "{{organisationId}}",
                "serviceId": "{{TestClientId}}",
                "roles": []
            }
            """;

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        // Act
        await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        handler.CapturedRequest.Should().NotBeNull();
        handler.CapturedRequest!.RequestUri!.ToString().Should().Be(
            $"{TestApiProxyUrl}/services/{TestClientId}/organisations/{organisationId}/users/{userId}");
        handler.CapturedRequest.Method.Should().Be(HttpMethod.Get);

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_IncludesBearerToken()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        var responseJson = $$"""
            {
                "userId": "{{userId}}",
                "organisationId": "{{organisationId}}",
                "serviceId": "{{TestClientId}}",
                "roles": []
            }
            """;

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        // Act
        await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        handler.CapturedRequest.Should().NotBeNull();
        handler.CapturedRequest!.Headers.Authorization.Should().NotBeNull();
        handler.CapturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        handler.CapturedRequest.Headers.Authorization.Parameter.Should().NotBeNullOrEmpty();

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_GeneratesValidJwtToken()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        var responseJson = $$"""
            {
                "userId": "{{userId}}",
                "organisationId": "{{organisationId}}",
                "serviceId": "{{TestClientId}}",
                "roles": []
            }
            """;

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        // Act
        await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        handler.CapturedRequest.Should().NotBeNull();
        var token = handler.CapturedRequest!.Headers.Authorization!.Parameter;

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be(TestClientId);
        jwtToken.Audiences.Should().Contain("signin.education.gov.uk");
        jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(10));

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_WithNullRolesInResponse_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        // Response with null roles
        var responseJson = "{\"userId\":\"user-123\",\"organisationId\":\"" + organisationId + "\",\"serviceId\":\"TestClientId\"}";

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        // Act
        var result = await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        result.Should().BeEmpty();

        httpClient.Dispose();
    }

    [Test]
    public async Task GetUserRolesAsync_TrimsTrailingSlashFromApiUrl()
    {
        // Arrange
        _mockConfiguration.Setup(x => x.APIServiceProxyUrl).Returns(TestApiProxyUrl + "/");
        var userId = "user-123";
        var organisationId = Guid.NewGuid();

        var responseJson = $$"""
            {
                "userId": "{{userId}}",
                "organisationId": "{{organisationId}}",
                "serviceId": "{{TestClientId}}",
                "roles": []
            }
            """;

        var (sut, handler, httpClient) = CreateSutWithTestHandler();
        handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        // Act
        await sut.GetUserRolesAsync(userId, organisationId);

        // Assert
        handler.CapturedRequest.Should().NotBeNull();
        handler.CapturedRequest!.RequestUri!.ToString().Should().NotContain("//services");

        httpClient.Dispose();
    }
}
