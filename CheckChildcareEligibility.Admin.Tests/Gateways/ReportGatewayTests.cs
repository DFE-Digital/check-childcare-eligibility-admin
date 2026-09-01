using System.Net;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Tests.Gateways
{
    internal class ReportGatewayTests
    {
        private Mock<IConfiguration> _configMock;
        private IHttpContextAccessor _httpContextAccessor;
        private HttpClient _httpClient;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<ILogger> _loggerMock;
        private DerivedReportGateway _sut;

        [SetUp]
        public void Setup()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger>();

            _loggerFactoryMock
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(_loggerMock.Object);

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

            _sut = new DerivedReportGateway(
                _loggerFactoryMock.Object,
                _httpClient,
                _configMock.Object,
                _httpContextAccessor);
        }
        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }
        [Test]
        public async Task Given_GetAllWorkingFamiliesEventsByEligibilityCode_When_CalledWithValidResponse_Should_ReturnWorkingFamilyEventByEligibilityCodeResponse()
        {
            // Arrange
            var responseContent = new WorkingFamilyEventByEligibilityCodeResponse();

            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    JsonConvert.SerializeObject(responseContent))
            };

            _httpMessageHandlerMock.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result =
                await _sut.GetAllWorkingFamiliesEventsByEligibilityCode("12345678901");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseContent);
        }
        [Test]
        public async Task Given_GetAllWorkingFamiliesEventsByEligibilityCode_WhenExceptionOccurs_ShouldReturnNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("API Error"));

            // Act
            var result = await _sut.GetAllWorkingFamiliesEventsByEligibilityCode("12345678901");

            // Assert
            result.Should().BeNull();
        }
    }
}
