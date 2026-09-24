using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Usecases;
using FluentAssertions;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class GetFosterChildUseCaseTests
{
    private Mock<IFosterFamiliesGateway> _gatewayMock = null!;
    private GetFosterChildUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _sut = new GetFosterChildUseCase(_gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenFosterChildIdIsEmpty_ThrowsValidationException()
    {
        await FluentActions.Invoking(async () => await _sut.Execute(Guid.Empty, 123))
            .Should().ThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
    }

    [Test]
    public async Task Execute_WhenGatewayReturnsNull_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _gatewayMock.Setup(x => x.GetFosterChild(id, 123, false)).ReturnsAsync((FosterChildResponse)null!);

        await FluentActions.Invoking(async () => await _sut.Execute(id, 123))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Foster child {id} not found");
    }

    [Test]
    public async Task Execute_WhenGatewayReturnsResponse_ReturnsResponse()
    {
        var id = Guid.NewGuid();
        var expected = new FosterChildResponse
        {
            FosterChildId = id,
            ChildFirstName = "Child",
            ChildLastName = "Doe",
            ChildDateOfBirth = DateTime.Today.AddYears(-4),
            ChildPostCode = "SW1A 1AA",
            EligibilityCode = "ABC123"
        };

        _gatewayMock.Setup(x => x.GetFosterChild(id, 123, true)).ReturnsAsync(expected);

        var result = await _sut.Execute(id, 123, true);

        result.Should().BeEquivalentTo(expected);
    }
}
