using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Usecases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class SearchFosterFamiliesRecordsUseCaseTests
{
    private Mock<IFosterFamiliesGateway> _gatewayMock;
    private Mock<ILogger<SearchFosterFamiliesRecordsUseCase>> _loggerMock;
    private SearchFosterFamiliesRecordsUseCase _sut;

    [SetUp]
    public void SetUp()
    {
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _loggerMock = new Mock<ILogger<SearchFosterFamiliesRecordsUseCase>>();
        _sut = new SearchFosterFamiliesRecordsUseCase(_loggerMock.Object, _gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenNinoFilterIsProvided_ShouldPassFilterToGateway()
    {
        var request = new FosterFamiliesSearchRequest(2, 10, "AB123456C");
        var expectedResponse = new FosterFamiliesSearchResponse
        {
            PageNumber = 2,
            PageSize = 10,
            TotalNumberOfRecords = 1,
            Data =
            [
                new FosterFamiliesSearchItemResponse { FosterCarerId = Guid.NewGuid() }
            ]
        };

        _gatewayMock
            .Setup(x => x.GetFosterFamiliesSearchRecords(2, 10, "AB123456C"))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.Execute(request);

        result.Should().BeSameAs(expectedResponse);
        _gatewayMock.Verify(x => x.GetFosterFamiliesSearchRecords(2, 10, "AB123456C"), Times.Once);
    }

    [Test]
    public async Task Execute_WhenGatewayReturnsNull_ShouldThrowApplicationException()
    {
        var request = new FosterFamiliesSearchRequest(1, 10, "AB123456C");

        _gatewayMock
            .Setup(x => x.GetFosterFamiliesSearchRecords(1, 10, "AB123456C"))
            .ReturnsAsync((FosterFamiliesSearchResponse)null!);

        var act = async () => await _sut.Execute(request);

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Failed to load foster families search results");
    }
}
