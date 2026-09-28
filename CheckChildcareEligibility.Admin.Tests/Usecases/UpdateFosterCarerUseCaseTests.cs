using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Usecases;
using FluentAssertions;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class UpdateFosterCarerUseCaseTests
{
    [TestCase(true, "ab123456c")]
    [TestCase(false, "ab123456c")]
    public async Task Execute_Should_Forward_Canonical_Nino(
        bool partner,
        string input)
    {
        var gateway = new Mock<IFosterFamiliesGateway>(MockBehavior.Strict);
        var sut = new UpdateFosterCarerUseCase(gateway.Object);
        var id = Guid.NewGuid();
        var request = BuildRequest(partner, input);

        gateway
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

        await sut.Execute(id, 201, request);

        gateway.VerifyAll();
        gateway.Verify(
            g => g.UpdateFosterCarer(id, 201, request),
            Times.Once);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Execute_Should_Reject_Invalid_Nino_Without_Calling_Gateway(
        bool partner)
    {
        var gateway = new Mock<IFosterFamiliesGateway>(MockBehavior.Strict);
        var sut = new UpdateFosterCarerUseCase(gateway.Object);
        var request = BuildRequest(partner, "BG123456C");

        Func<Task> act = () =>
            sut.Execute(Guid.NewGuid(), 201, request);

        var thrown = await act.Should()
            .ThrowAsync<FluentValidation.ValidationException>();

        var expectedProperty = partner
            ? "PartnerNationalInsuranceNumber"
            : "CarerNationalInsuranceNumber";

        thrown.Which.Errors.Should().NotBeEmpty();
        thrown.Which.Errors.Should().OnlyContain(
            error => error.PropertyName == expectedProperty);

        gateway.Verify(
            g => g.UpdateFosterCarer(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<UpdateFosterCarerRequest>()),
            Times.Never);
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