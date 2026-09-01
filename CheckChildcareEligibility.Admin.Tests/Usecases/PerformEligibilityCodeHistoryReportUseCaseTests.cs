using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.UseCases;
using FluentAssertions;

using Microsoft.Extensions.Logging;
using Moq;
namespace CheckChildcareEligibility.Admin.Tests.Usecases
{
    [TestFixture]
    public class PerformEligibilityCodeHistoryReportUseCaseTests
    {
        private Mock<IReportGateway> _reportGatewayMock;
        private Mock<ILogger<PerformEligibilityCodeHistoryReportUseCase>> _loggerMock;

        private PerformEligibilityCodeHistoryReportUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _reportGatewayMock = new Mock<IReportGateway>();
            _loggerMock = new Mock<ILogger<PerformEligibilityCodeHistoryReportUseCase>>();

            _sut = new PerformEligibilityCodeHistoryReportUseCase(
                _loggerMock.Object,
                _reportGatewayMock.Object);
        }

        [Test]
        public async Task Execute_WithValidEligibilityCode_ShouldReturnResponse()
        {
            // Arrange
            var eligibilityCode = "12345678901";
            var expectedResponse = new WorkingFamilyEventByEligibilityCodeResponse();

            _reportGatewayMock.Setup(x => x.GetAllWorkingFamiliesEventsByEligibilityCode(eligibilityCode)).ReturnsAsync(expectedResponse);

            // Act
            var response = await _sut.Execute(eligibilityCode);

            // Assert
            response.Should().BeEquivalentTo(expectedResponse);
            _reportGatewayMock.Verify(x => x.GetAllWorkingFamiliesEventsByEligibilityCode(eligibilityCode),Times.Once);
        }
        [Test]
        public async Task Execute_WhenEligibilityCodeIsEmpty_ShouldThrowArgumentException()
        {
            // Act
            Func<Task> act = async () => await _sut.Execute(string.Empty);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Eligibility code is required.*");
        }
    }
}
