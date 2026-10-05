using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Usecases;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ReconfirmFosterCodeUseCaseTests
{
    private Mock<IFosterFamiliesGateway> _gatewayMock = null!;
    private ReconfirmFosterCodeUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _sut = new ReconfirmFosterCodeUseCase(
            new Mock<ILogger<ReconfirmFosterCodeUseCase>>().Object,
            _gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        await FluentActions.Invoking(() => _sut.Execute(Guid.NewGuid(), null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsInvalid_ThrowsValidationExceptionAndDoesNotCallGateway()
    {
        var request = new FosterChildReconfirmRequest { SubmissionDate = DateTime.Today.AddDays(1) };

        await FluentActions.Invoking(() => _sut.Execute(Guid.NewGuid(), request))
            .Should().ThrowAsync<ValidationException>();

        _gatewayMock.Verify(x => x.ReconfirmFosterChild(It.IsAny<Guid>(), It.IsAny<FosterChildReconfirmRequest>()), Times.Never);
    }

    [Test]
    public async Task Execute_WhenRequestIsValid_ReturnsResponseAndCallsGateway()
    {
        var childId = Guid.NewGuid();
        var request = new FosterChildReconfirmRequest
        {
            EligibilityCode = "ABC123",
            SubmissionDate = DateTime.Today.AddDays(-1)
        };
        var expected = new FosterChildResponse { FosterChildId = childId, EligibilityCode = "DEF456" };
        _gatewayMock.Setup(x => x.ReconfirmFosterChild(childId, request)).ReturnsAsync(expected);

        var result = await _sut.Execute(childId, request);

        result.Should().BeEquivalentTo(expected);
        _gatewayMock.Verify(x => x.ReconfirmFosterChild(childId, request), Times.Once);
    }
}
