using System.Net;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Gateways.Tests.Check;

internal class CheckGatewayTests
{
    private Mock<IConfiguration> _configMock;
    private HttpClient _httpClient;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<ILoggerFactory> _loggerFactoryMock;
    private Mock<ILogger> _loggerMock;
    private DerivedCheckGateway _sut;

    [SetUp]
    public void Setup()
    {
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerMock = new Mock<ILogger>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(x => x["Api:AuthorisationUsername"]).Returns("SomeValue");
        _configMock.Setup(x => x["Api:AuthorisationPassword"]).Returns("SomeValue");
        _configMock.Setup(x => x["Api:AuthorisationEmail"]).Returns("SomeValue");
        _configMock.Setup(x => x["Api:AuthorisationScope"]).Returns("SomeValue");

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:7000")
        };

        _sut = new DerivedCheckGateway(_loggerFactoryMock.Object, _httpClient, _configMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }


    [Test]
    public async Task Given_GetStatus_When_CalledWithValidResponse_Should_ReturnCheckEligibilityStatusResponse()
    {
        // Arrange
        var responseBody = new CheckEligibilityResponse
        {
            Links = new CheckEligibilityResponseLinks { Get_EligibilityCheck = "EligibilityCheckLink" }
        };
        var responseContent = new CheckEligibilityStatusResponse();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(responseContent))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.GetStatus(responseBody);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(responseContent);
    }

    [Test]
    public async Task Given_PostCheck_When_CalledWithValidRequest_Should_ReturnCheckEligibilityResponse()
    {
        // Arrange
        var requestBody = new CheckEligibilityRequest();
        var responseContent = new CheckEligibilityResponse();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(responseContent))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.PostCheck(requestBody);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(responseContent);
    }

    [Test]
    public void Given_GetStatus_When_CalledWithNullInput_Should_ThrowArgumentNullException()
    {
        // Act
        var result = async () => await _sut.GetStatus(null);


        // Assert
        var resultAsResponse = result.As<Task<CheckEligibilityStatusResponse>>();
        resultAsResponse.Should().BeNull();

        result.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Given_GetStatus_When_GivenInAnInvalidValue_ShouldReturnNull()
    {
        // Arrange

        // Assert
        var result = _sut.GetStatus(new CheckEligibilityResponse
        {
            Data = new StatusValue
            {
                Status = "unknown"
            },
            Links = new CheckEligibilityResponseLinks
            {
                Get_EligibilityCheck = "link"
            }
        });

        // Act
        result.Result.Should().Be(null);
    }

    [Test]
    public async Task
        Given_PostCheck_When_ApiReturnsUnauthorized_Should_LogApiErrorAnd_Throw_UnauthorizedAccessException()
    {
        // Arrange
        var requestBody = new CheckEligibilityRequest();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent("")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _sut.PostCheck(requestBody);

        // Assert
        await act.Should().ThrowExactlyAsync<UnauthorizedAccessException>();
    }


    [Test]
    public async Task Given_GetStatus_When_ApiReturnsUnauthorized_Should_LogApiErrorAndThrowExecption()
    {
        // Arrange
        var responseBody = new CheckEligibilityResponse
        {
            Links = new CheckEligibilityResponseLinks { Get_EligibilityCheck = "EligibilityCheckLink" }
        };
        var responseContent = new CheckEligibilityStatusResponse();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent(JsonConvert.SerializeObject(responseContent))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = _sut.GetStatus(responseBody);

        // Assert
        result.Result.Data.Should().BeNull();
        _sut.apiErrorCount.Should().Be(1);
    }
    
    [Test]
    public async Task Given_GetBulkCheckProgress_When_CalledWithValidUrl_Should_ReturnBulkStatusResponse()
    {
        // Arrange
        const string bulkCheckUrl = "bulk-check/sample-url";
        var responseContent = new CheckEligibilityBulkStatusResponse();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(responseContent))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.GetBulkCheckProgress(bulkCheckUrl);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(responseContent);
    }

    [Test]
    public async Task Given_GetBulkCheckProgress_When_ApiThrowsException_Should_ReturnNull()
    {
        // Arrange
        const string bulkCheckUrl = "bulk-check/error-url";
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent("Error")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Test exception"));

        // Act
        var result = await _sut.GetBulkCheckProgress(bulkCheckUrl);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Given_GetBulkCheckResults_When_CalledWithValidUrl_Should_ReturnBulkResponse()
    {
        // Arrange
        const string resultsUrl = "bulk-check/results-url";
        var responseContent = new CheckEligibilityBulkResponse();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(responseContent))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.GetBulkCheckResults(resultsUrl);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(responseContent);
    }

    [Test]
    public async Task Given_GetBulkCheckResults_When_ApiThrowsException_Should_PropagateException()
    {
        // Arrange
        const string resultsUrl = "bulk-check/error-url";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Test exception"));

        // Act
        Func<Task> act = async () => await _sut.GetBulkCheckResults(resultsUrl);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Test exception");
    }

    [Test]
    public async Task Given_PostBulkCheck_When_CalledWithValidRequest_Should_ReturnBulkResponse()
    {
        // Arrange
        var requestBody = new CheckEligibilityRequestBulk
        {
            Data = new List<CheckEligibilityRequestData>
            {
                new CheckEligibilityRequestData(CheckEligibilityType.FreeSchoolMeals)
            }
        };
        var responseContent = new CheckEligibilityResponseBulk();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(responseContent))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.PostBulkCheck(requestBody);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(responseContent);
    }

    [Test]
    public async Task Given_PostBulkCheck_When_ApiReturnsUnauthorized_Should_ThrowUnauthorizedException()
    {
        // Arrange
        var requestBody = new CheckEligibilityRequestBulk
        {
            Data = new List<CheckEligibilityRequestData>
            {
                new CheckEligibilityRequestData(CheckEligibilityType.FreeSchoolMeals)
            }
        };
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent("")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _sut.PostBulkCheck(requestBody);

        // Assert
        await act.Should().ThrowExactlyAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Given_PostBulkCheck_When_NullOrEmptyData_Should_HandleErrorUrl()
    {
        // Arrange
        var requestBody = new CheckEligibilityRequestBulk
        {
            Data = new List<CheckEligibilityRequestData>() // Empty data
        };
        
        // We expect an exception since BulkCheckUrl returns "error"
        // when data is null or empty, and there's no handler for this URL

        // Act
        Func<Task> act = async () => await _sut.PostBulkCheck(requestBody);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}