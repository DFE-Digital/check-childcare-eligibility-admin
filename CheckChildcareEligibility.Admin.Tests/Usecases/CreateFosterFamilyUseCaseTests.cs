using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Usecases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class CreateFosterFamilyUseCaseTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task Execute_Should_Reject_Invalid_Nino_Without_Calling_Gateway(
        bool invalidPartner)
    {
        var gateway = new Mock<IFosterFamiliesGateway>(MockBehavior.Strict);
        var logger = new Mock<ILogger<CreateFosterFamilyUseCase>>();
        var sut = new CreateFosterFamilyUseCase(logger.Object, gateway.Object);

        var request = new FosterFamilyRequest
        {
            SubmissionDate = DateTime.Today,
            HasPartner = true,
            FosterCarer = new FosterCarerRequest
            {
                CarerFirstName = "Alex",
                CarerLastName = "Foster",
                CarerDateOfBirth = new DateTime(1980, 2, 3),
                CarerNationalInsuranceNumber =
                    invalidPartner ? "AB123456C" : "BG123456C"
            },
            Partner = new FosterPartnerRequest
            {
                PartnerFirstName = "Pat",
                PartnerLastName = "Foster",
                PartnerDateOfBirth = new DateTime(1982, 4, 5),
                PartnerNationalInsuranceNumber =
                    invalidPartner ? "BG123456C" : "CE123456A"
            },
            FosterChild = new FosterChildRequest
            {
                ChildFirstName = "Casey",
                ChildLastName = "Foster",
                ChildDateOfBirth = DateTime.Today.AddYears(-2),
                ChildPostCode = "SW1A 1AA"
            }
        };

        Func<Task> act = () => sut.Execute(request, 201);

        var thrown = await act.Should()
            .ThrowAsync<FluentValidation.ValidationException>();

        var expectedProperty = invalidPartner
            ? "Partner.PartnerNationalInsuranceNumber"
            : "FosterCarer.CarerNationalInsuranceNumber";

        thrown.Which.Errors.Should().NotBeEmpty();
        thrown.Which.Errors.Should().OnlyContain(
            error => error.PropertyName == expectedProperty);

        gateway.Verify(
            g => g.CreateFosterFamily(It.IsAny<FosterFamilyRequest>()),
            Times.Never);
    }

    [TestCase("ab 12 34 56 c", "ce 12 34 56 a")]
    [TestCase("ab-12.34/56c", "ce-12.34/56a")]
    [TestCase("ab\t12\r\n3456c", "ce\t12\r\n3456a")]
    public async Task Execute_Should_Forward_Canonical_Carer_And_Partner_Ninos(
    string carerNino,
    string partnerNino)
    {
        var gateway = new Mock<IFosterFamiliesGateway>(MockBehavior.Strict);
        var logger = new Mock<ILogger<CreateFosterFamilyUseCase>>();
        var sut = new CreateFosterFamilyUseCase(logger.Object, gateway.Object);

        var request = new FosterFamilyRequest
        {
            SubmissionDate = DateTime.Today,
            HasPartner = true,
            FosterCarer = new FosterCarerRequest
            {
                CarerFirstName = "Alex",
                CarerLastName = "Foster",
                CarerDateOfBirth = new DateTime(1980, 2, 3),
                CarerNationalInsuranceNumber = carerNino
            },
            Partner = new FosterPartnerRequest
            {
                PartnerFirstName = "Pat",
                PartnerLastName = "Foster",
                PartnerDateOfBirth = new DateTime(1982, 4, 5),
                PartnerNationalInsuranceNumber = partnerNino
            },
            FosterChild = new FosterChildRequest
            {
                ChildFirstName = "Casey",
                ChildLastName = "Foster",
                ChildDateOfBirth = DateTime.Today.AddYears(-2),
                ChildPostCode = "SW1A 1AA"
            }
        };

        // Request properties preserve input until the use case processes it.
        request.FosterCarer.CarerNationalInsuranceNumber.Should().Be(carerNino);
        request.Partner.PartnerNationalInsuranceNumber.Should().Be(partnerNino);

        var expectedResponse = new FosterFamilyCreatedResponse();

        gateway
            .Setup(g => g.CreateFosterFamily(
                It.Is<FosterFamilyRequest>(r =>
                    r.FosterCarer.CarerNationalInsuranceNumber == "AB123456C" &&
                    r.FosterCarer.LocalAuthorityID == 201 &&
                    r.Partner != null &&
                    r.Partner.PartnerNationalInsuranceNumber == "CE123456A")))
            .ReturnsAsync(expectedResponse);

        var result = await sut.Execute(request, 201);

        result.Should().BeSameAs(expectedResponse);

        gateway.Verify(
            g => g.CreateFosterFamily(It.IsAny<FosterFamilyRequest>()),
            Times.Once);
        gateway.VerifyAll();
    }
}