using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Usecases;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class UpdateFosterCarerUseCaseTests
{
    private Mock<IFosterFamiliesGateway> _gatewayMock = null!;
    private UpdateFosterCarerUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _sut = new UpdateFosterCarerUseCase(_gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenFosterCarerIdIsEmpty_ThrowsValidationException()
    {
        var request = BuildValidUpdateFosterCarerRequest();

        await FluentActions.Invoking(async () => await _sut.Execute(Guid.Empty, 123, request))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        await FluentActions.Invoking(async () => await _sut.Execute(Guid.NewGuid(), 123, null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Execute_WhenPartnerRequestIsInvalid_ThrowsValidationException()
    {
        var request = new UpdateFosterCarerRequest
        {
            FosterCarerRequest = new FosterCarerRequest
            {
                CarerFirstName = "Jane",
                CarerLastName = "Doe",
                CarerDateOfBirth = DateTime.Today.AddYears(-30)
            },
            FosterPartnerRequest = new FosterPartnerRequest
            {
                PartnerFirstName = "",
                PartnerLastName = "Smith",
                PartnerDateOfBirth = DateTime.Today.AddYears(-28)
            }
        };

        await FluentActions.Invoking(async () => await _sut.Execute(Guid.NewGuid(), 123, request))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsValid_CallsGateway()
    {
        var id = Guid.NewGuid();
        var request = BuildValidUpdateFosterCarerRequest();

        await _sut.Execute(id, 456, request);

        _gatewayMock.Verify(x => x.UpdateFosterCarer(id, 456, request), Times.Once);
    }

    private static UpdateFosterCarerRequest BuildValidUpdateFosterCarerRequest()
    {
        return new UpdateFosterCarerRequest
        {
            FosterCarerRequest = new FosterCarerRequest
            {
                CarerFirstName = "Jane",
                CarerLastName = "Doe",
                CarerDateOfBirth = DateTime.Today.AddYears(-30),
                CarerNationalInsuranceNumber = "AA123456A"
            },
            FosterPartnerRequest = new FosterPartnerRequest
            {
                PartnerFirstName = "John",
                PartnerLastName = "Smith",
                PartnerDateOfBirth = DateTime.Today.AddYears(-28),
                PartnerNationalInsuranceNumber = "BB123456B"
            }
        };
    }
}
