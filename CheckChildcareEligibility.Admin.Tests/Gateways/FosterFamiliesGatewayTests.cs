using System.Net;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Tests.Gateways;

[TestFixture]
public class FosterFamiliesGatewayTests
{
    private Mock<IConfiguration> _configurationMock = null!;
    private HttpClient _httpClient = null!;
    private Mock<HttpMessageHandler> _handlerMock = null!;
    private FosterFamiliesGateway _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);

        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["Api:AuthorisationUsername"]).Returns("username");
        _configurationMock.Setup(x => x["Api:AuthorisationPassword"]).Returns("password");
        _configurationMock.Setup(x => x["Api:AuthorisationEmail"]).Returns("email");
        _configurationMock.Setup(x => x["Api:AuthorisationScope"]).Returns("local_authority");

        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:7000")
        };

        _sut = new FosterFamiliesGateway(
            loggerFactoryMock.Object,
            _httpClient,
            _configurationMock.Object,
            new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test]
    public async Task GetFosterFamiliesSearchRecords_WhenApiReturnsResponse_ReturnsResponseAndBuildsQuery()
    {
        var expected = new FosterFamiliesSearchResponse { PageNumber = 2, PageSize = 10, TotalNumberOfRecords = 1 };
        HttpRequestMessage? capturedRequest = null;
        SetupResponse(HttpStatusCode.OK, expected, request => capturedRequest = request);

        var result = await _sut.GetFosterFamiliesSearchRecords(2, 10, "AB123456C");

        result.Should().BeEquivalentTo(expected);
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.PathAndQuery.Should().Be("/foster-family/search?pageNumber=2&pageSize=10&ninoFilter=AB123456C");
    }

    [Test]
    public async Task GetFosterFamiliesSearchRecords_WhenApiThrowsException_ReturnsNull()
    {
        SetupException(new HttpRequestException("API error"));

        var result = await _sut.GetFosterFamiliesSearchRecords(1, 10);

        result.Should().BeNull();
    }

    [Test]
    public async Task CreateFosterFamily_WhenApiReturnsResponse_PostsRequestAndReturnsResponse()
    {
        var request = BuildFamilyRequest();
        var expected = new FosterFamilyCreatedResponse { FosterCarerId = Guid.NewGuid(), FosterChildId = Guid.NewGuid() };
        HttpRequestMessage? capturedRequest = null;
        SetupResponse(HttpStatusCode.OK, expected, requestMessage => capturedRequest = requestMessage);

        var result = await _sut.CreateFosterFamily(request);

        result.Should().BeEquivalentTo(expected);
        await AssertRequestAsync(capturedRequest!, HttpMethod.Post, "/foster-family", request);
    }

    [Test]
    public async Task CreateFosterFamily_WhenApiThrowsException_RethrowsException()
    {
        SetupException(new HttpRequestException("API error"));

        await FluentActions.Invoking(() => _sut.CreateFosterFamily(BuildFamilyRequest()))
            .Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task PreviewFosterFamilyCode_WhenApiReturnsResponse_PostsRequestAndReturnsResponse()
    {
        var request = BuildFamilyRequest();
        var expected = new FosterCodePreviewResponse { GracePeriodEndDate = DateTime.Today.AddDays(30) };
        HttpRequestMessage? capturedRequest = null;
        SetupResponse(HttpStatusCode.OK, expected, requestMessage => capturedRequest = requestMessage);

        var result = await _sut.PreviewFosterFamilyCode(request);

        result.Should().BeEquivalentTo(expected);
        await AssertRequestAsync(capturedRequest!, HttpMethod.Post, "/foster-family/preview", request);
    }

    [Test]
    public async Task GetFosterFamily_WhenApiReturnsResponse_UsesFamilyIdAndIncludeChildren()
    {
        var familyId = Guid.NewGuid();
        var expected = new FosterFamilyResponse { FosterCarerId = familyId, CarerFirstName = "Jane" };
        HttpRequestMessage? capturedRequest = null;
        SetupResponse(HttpStatusCode.OK, expected, request => capturedRequest = request);

        var result = await _sut.GetFosterFamily(familyId, true);

        result.Should().BeEquivalentTo(expected);
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.PathAndQuery.Should().Be($"/foster-family/{familyId}?includeChildren=True");
    }

    [Test]
    public async Task GetFosterChild_WhenApiReturnsResponse_UsesChildIdAndIncludeCarer()
    {
        var childId = Guid.NewGuid();
        var expected = new FosterChildResponse { FosterChildId = childId, ChildFirstName = "Child" };
        HttpRequestMessage? capturedRequest = null;
        SetupResponse(HttpStatusCode.OK, expected, request => capturedRequest = request);

        var result = await _sut.GetFosterChild(childId, true);

        result.Should().BeEquivalentTo(expected);
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.PathAndQuery.Should().Be($"/foster-family/child/{childId}?includeFosterCarer=True");
    }

    [Test]
    public async Task UpdateFosterCarer_WhenApiReturnsSuccess_PatchesFamilyRequest()
    {
        var familyId = Guid.NewGuid();
        var request = new UpdateFosterCarerRequest
        {
            FosterCarerRequest = new FosterCarerRequest { CarerFirstName = "Jane", CarerLastName = "Smith" }
        };
        HttpRequestMessage? capturedRequest = null;
        SetupResponse(HttpStatusCode.OK, new { }, requestMessage => capturedRequest = requestMessage);

        await _sut.UpdateFosterCarer(familyId, request);

        await AssertRequestAsync(capturedRequest!, HttpMethod.Patch, $"/foster-family/{familyId}", request);
    }

    [Test]
    public async Task UpdateFosterChild_WhenApiReturnsSuccess_PatchesChildRequest()
    {
        var childId = Guid.NewGuid();
        var request = new UpdateFosterChildRequest
        {
            FosterChildRequest = new FosterChildRequest { ChildFirstName = "Child", ChildLastName = "Smith" }
        };
        HttpRequestMessage? capturedRequest = null;
        SetupResponse(HttpStatusCode.OK, new { }, requestMessage => capturedRequest = requestMessage);

        await _sut.UpdateFosterChild(childId, request);

        await AssertRequestAsync(capturedRequest!, HttpMethod.Patch, $"/foster-family/child/{childId}", request);
    }

    [Test]
    public async Task PreviewFosterChildReconfirm_WhenApiReturnsResponse_PostsReconfirmRequest()
    {
        var childId = Guid.NewGuid();
        var request = new FosterChildReconfirmRequest
        {
            EligibilityCode = "ABC123",
            SubmissionDate = DateTime.Today.AddDays(-1)
        };
        var expected = new FosterCodePreviewResponse { GracePeriodEndDate = DateTime.Today.AddDays(30) };
        HttpRequestMessage? capturedRequest = null;
        SetupResponse(HttpStatusCode.OK, expected, requestMessage => capturedRequest = requestMessage);

        var result = await _sut.PreviewFosterChildReconfirm(childId, request);

        result.Should().BeEquivalentTo(expected);
        await AssertRequestAsync(capturedRequest!, HttpMethod.Post, $"/foster-family/child/{childId}/preview-reconfirm", request);
    }

    [Test]
    public async Task ReconfirmFosterChild_WhenApiReturnsResponse_PostsReconfirmRequestAndReturnsResponse()
    {
        var childId = Guid.NewGuid();
        var request = new FosterChildReconfirmRequest
        {
            EligibilityCode = "ABC123",
            SubmissionDate = DateTime.Today.AddDays(-1)
        };
        var expected = new FosterChildResponse { FosterChildId = childId, EligibilityCode = "DEF456" };
        HttpRequestMessage? capturedRequest = null;
        SetupResponse(HttpStatusCode.OK, expected, requestMessage => capturedRequest = requestMessage);

        var result = await _sut.ReconfirmFosterChild(childId, request);

        result.Should().BeEquivalentTo(expected);
        await AssertRequestAsync(capturedRequest!, HttpMethod.Post, $"/foster-family/child/{childId}/reconfirm", request);
    }

    private void SetupResponse<T>(HttpStatusCode statusCode, T response, Action<HttpRequestMessage>? capture = null)
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capture?.Invoke(request))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonConvert.SerializeObject(response))
            });
    }

    private void SetupException(Exception exception)
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
    }

    private static async Task AssertRequestAsync<T>(HttpRequestMessage request, HttpMethod method, string path, T expectedBody)
    {
        request.Method.Should().Be(method);
        request.RequestUri!.PathAndQuery.Should().Be(path);
        request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
        var body = await request.Content.ReadAsStringAsync();
        body.Should().Be(JsonConvert.SerializeObject(expectedBody));
    }

    private static FosterFamilyRequest BuildFamilyRequest()
    {
        return new FosterFamilyRequest
        {
            FosterCarer = new FosterCarerRequest
            {
                CarerFirstName = "Jane",
                CarerLastName = "Smith",
                CarerDateOfBirth = DateTime.Today.AddYears(-30),
                CarerNationalInsuranceNumber = "AB123456C"
            },
            FosterChild = new FosterChildRequest
            {
                ChildFirstName = "Child",
                ChildLastName = "Smith",
                ChildDateOfBirth = DateTime.Today.AddYears(-4),
                ChildPostCode = "SW1A 1AA"
            },
            SubmissionDate = DateTime.Today.AddDays(-1)
        };
    }
}
