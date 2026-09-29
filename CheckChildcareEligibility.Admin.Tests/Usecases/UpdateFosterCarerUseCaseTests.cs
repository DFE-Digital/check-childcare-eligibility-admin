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

        await FluentActions.Invoking(
                async () => await _sut.Execute(Guid.Empty, 123, request))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        await FluentActions.Invoking(
                async () => await _sut.Execute(Guid.NewGuid(), 123, null!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
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

        await FluentActions.Invoking(
                async () => await _sut.Execute(Guid.NewGuid(), 123, request))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsValid_CallsGateway()
    {
        var id = Guid.NewGuid();
        var request = BuildValidUpdateFosterCarerRequest();

        await _sut.Execute(id, 456, request);

        _gatewayMock.Verify(
            x => x.UpdateFosterCarer(id, 456, request),
            Times.Once);
    }

    [TestCase(true, "ab123456c")]
    [TestCase(false, "ab123456c")]
    public async Task Execute_Should_Forward_Canonical_Nino(
        bool partner,
        string input)
    {
        var id = Guid.NewGuid();
        var request = BuildRequest(partner, input);

        _gatewayMock
            .Setup(g => g.UpdateFosterCarer(
                id,
                201,
                It.Is<UpdateFosterCarerRequest>(r =>
                    partner
                        ? r.FosterPartnerRequest != null &&
                          r.FosterPartnerRequest
                              .PartnerNationalInsuranceNumber == "AB123456C"
                        : r.FosterCarerRequest != null &&
                          r.FosterCarerRequest
                              .CarerNationalInsuranceNumber == "AB123456C")))
            .Returns(Task.CompletedTask);

        await _sut.Execute(id, 201, request);

        _gatewayMock.VerifyAll();

        _gatewayMock.Verify(
            g => g.UpdateFosterCarer(id, 201, request),
            Times.Once);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Execute_Should_Reject_Invalid_Nino_Without_Calling_Gateway(
        bool partner)
    {
        var request = BuildRequest(partner, "BG123456C");

        Func<Task> act = () =>
            _sut.Execute(Guid.NewGuid(), 201, request);

        var thrown = await act.Should()
            .ThrowAsync<ValidationException>();

        var expectedProperty = partner
            ? "PartnerNationalInsuranceNumber"
            : "CarerNationalInsuranceNumber";

        thrown.Which.Errors.Should().NotBeEmpty();

        thrown.Which.Errors.Should().OnlyContain(
            error => error.PropertyName == expectedProperty);

        _gatewayMock.Verify(
            g => g.UpdateFosterCarer(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<UpdateFosterCarerRequest>()),
            Times.Never);
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

    private static UpdateFosterCarerRequest BuildRequest(
        bool partner,
        string nino)
    {
        return partner
            ? new UpdateFosterCarerRequest
            {
                FosterPartnerRequest = new FosterPartnerRequest
                {
                    PartnerFirstName = "Pat",
                    PartnerLastName = "Foster",
                    PartnerDateOfBirth = new DateTime(1982, 4, 5),
                    PartnerNationalInsuranceNumber = nino
                }
            }
            : new UpdateFosterCarerRequest
            {
                FosterCarerRequest = new FosterCarerRequest
                {
                    CarerFirstName = "Alex",
                    CarerLastName = "Foster",
                    CarerDateOfBirth = new DateTime(1980, 2, 3),
                    CarerNationalInsuranceNumber = nino
                }
            };
    }
}
