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
    private Mock<ILogger<SearchFosterFamiliesRecordsUseCase>> _loggerMock = null!;
    private Mock<IFosterFamiliesGateway> _gatewayMock = null!;
    private SearchFosterFamiliesRecordsUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<SearchFosterFamiliesRecordsUseCase>>();
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _sut = new SearchFosterFamiliesRecordsUseCase(_loggerMock.Object, _gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenGatewayReturnsResponse_ReturnsResponse()
    {
        var request = new FosterFamiliesSearchRequest(1, 20);
        var expected = new FosterFamiliesSearchResponse
        {
            PageNumber = 1,
            PageSize = 20,
            TotalNumberOfRecords = 1,
            Data = new[]
            {
                new FosterFamiliesSearchItemResponse
                {
                    FosterCarerId = Guid.NewGuid(),
                    CarerName = "Jane Doe",
                    ChildName = "Child Doe"
                }
            }
        };

        _gatewayMock.Setup(x => x.GetFosterFamiliesSearchRecords(1, 20)).ReturnsAsync(expected);

        var result = await _sut.Execute(request);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task Execute_WhenGatewayReturnsNull_ThrowsApplicationException()
    {
        var request = new FosterFamiliesSearchRequest(1, 20);

        _gatewayMock.Setup(x => x.GetFosterFamiliesSearchRecords(1, 20)).ReturnsAsync((FosterFamiliesSearchResponse)null!);

        await FluentActions.Invoking(async () => await _sut.Execute(request))
            .Should().ThrowAsync<ApplicationException>()
            .WithMessage("Failed to load foster families search results");
    }
}
