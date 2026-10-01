using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.UseCases;
using FluentAssertions;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class GetFosterFamilyUseCaseTests
{
    private Mock<IFosterFamiliesGateway> _gatewayMock = null!;
    private GetFosterFamilyUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _sut = new GetFosterFamilyUseCase(_gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenFosterCarerIdIsEmpty_ThrowsValidationException()
    {
        await FluentActions.Invoking(async () => await _sut.Execute(Guid.Empty))
            .Should().ThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
    }

    [Test]
    public async Task Execute_WhenGatewayReturnsNull_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _gatewayMock.Setup(x => x.GetFosterFamily(id, false)).ReturnsAsync((FosterFamilyResponse)null!);

        await FluentActions.Invoking(async () => await _sut.Execute(id))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Foster carer {id} not found");
    }

    [Test]
    public async Task Execute_WhenGatewayReturnsResponse_ReturnsResponse()
    {
        var id = Guid.NewGuid();
        var expected = new FosterFamilyResponse
        {
            FosterCarerId = id,
            CarerFirstName = "Jane",
            CarerLastName = "Doe",
            CarerDateOfBirth = DateTime.Today.AddYears(-30),
            CarerNationalInsuranceNumber = "AA123456A",
            SubmissionDate = DateTime.Today.AddDays(-2)
        };

        _gatewayMock.Setup(x => x.GetFosterFamily(id, true)).ReturnsAsync(expected);

        var result = await _sut.Execute(id, true);

        result.Should().BeEquivalentTo(expected);
    }
}
