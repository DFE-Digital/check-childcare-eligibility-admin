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
public class CreateFosterFamilyUseCaseTests
{
    private Mock<ILogger<CreateFosterFamilyUseCase>> _loggerMock = null!;
    private Mock<IFosterFamiliesGateway> _gatewayMock = null!;
    private CreateFosterFamilyUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<CreateFosterFamilyUseCase>>();
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _sut = new CreateFosterFamilyUseCase(
            _loggerMock.Object,
            _gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        await FluentActions.Invoking(async () => await _sut.Execute(null!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsInvalid_ThrowsValidationException()
    {
        var request = new FosterFamilyRequest
        {
            FosterCarer = new FosterCarerRequest
            {
                CarerFirstName = "",
                CarerLastName = "Doe",
                CarerDateOfBirth = DateTime.Today.AddYears(-30),
                CarerNationalInsuranceNumber = "AA123456A"
            },
            FosterChild = new FosterChildRequest
            {
                ChildFirstName = "Child",
                ChildLastName = "Doe",
                ChildDateOfBirth = DateTime.Today.AddYears(-4),
                ChildPostCode = "SW1A 1AA"
            },
            SubmissionDate = DateTime.Today.AddDays(-1)
        };

        await FluentActions.Invoking(async () => await _sut.Execute(request))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsValid_ReturnsCreatedResponse()
    {
        var request = BuildValidFosterFamilyRequest();
        var expected = new FosterFamilyCreatedResponse
        {
            FosterCarerId = Guid.NewGuid(),
            FosterChildId = Guid.NewGuid()
        };

        _gatewayMock
            .Setup(x => x.CreateFosterFamily(request))
            .ReturnsAsync(expected);

        var result = await _sut.Execute(request);
        result.Should().BeEquivalentTo(expected);

        _gatewayMock.Verify(x => x.CreateFosterFamily(request),
            Times.Once);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Execute_Should_Reject_Invalid_Nino_Without_Calling_Gateway(
        bool invalidPartner)
    {
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

        Func<Task> act = () => _sut.Execute(request, 201);

        var thrown = await act.Should()
            .ThrowAsync<ValidationException>();

        var expectedProperty = invalidPartner
            ? "Partner.PartnerNationalInsuranceNumber"
            : "FosterCarer.CarerNationalInsuranceNumber";

        thrown.Which.Errors.Should().NotBeEmpty();
        thrown.Which.Errors.Should().OnlyContain(
            error => error.PropertyName == expectedProperty);

        _gatewayMock.Verify(
            g => g.CreateFosterFamily(It.IsAny<FosterFamilyRequest>()),
            Times.Never);
    }

    [TestCase("ab123456c", "ce123456a")]
    public async Task Execute_Should_Forward_Canonical_Carer_And_Partner_Ninos(
        string carerNino,
        string partnerNino)
    {
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

        request.FosterCarer.CarerNationalInsuranceNumber.Should().Be(carerNino);
        request.Partner.PartnerNationalInsuranceNumber.Should().Be(partnerNino);

        var expectedResponse = new FosterFamilyCreatedResponse();

        _gatewayMock
            .Setup(g => g.CreateFosterFamily(
                It.Is<FosterFamilyRequest>(r =>
                    r.FosterCarer.CarerNationalInsuranceNumber == "AB123456C" &&
                    r.FosterCarer.LocalAuthorityID == 201 &&
                    r.Partner != null &&
                    r.Partner.PartnerNationalInsuranceNumber == "CE123456A")))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.Execute(request);

        result.Should().BeSameAs(expectedResponse);

        _gatewayMock.Verify(
            g => g.CreateFosterFamily(It.IsAny<FosterFamilyRequest>()),
            Times.Once);

        _gatewayMock.VerifyAll();
    }

    [Test]
    public async Task Execute_WhenValidPartnerIsProvided_ReturnsCreatedResponseAndCallsGateway()
    {
        var request = BuildValidFosterFamilyRequest();
        request.HasPartner = true;
        request.Partner = new FosterPartnerRequest
        {
            PartnerFirstName = "John",
            PartnerLastName = "Doe",
            PartnerDateOfBirth = DateTime.Today.AddYears(-28),
            PartnerNationalInsuranceNumber = "BB123456B"
        };
        var expected = new FosterFamilyCreatedResponse { FosterCarerId = Guid.NewGuid(), FosterChildId = Guid.NewGuid() };

        _gatewayMock.Setup(x => x.CreateFosterFamily(request)).ReturnsAsync(expected);

        var result = await _sut.Execute(request);

        result.Should().BeEquivalentTo(expected);
        _gatewayMock.Verify(x => x.CreateFosterFamily(request), Times.Once);
    }

    [Test]
    public async Task Execute_WhenPartnerIsRequiredButMissing_ThrowsValidationExceptionAndDoesNotCallGateway()
    {
        var request = BuildValidFosterFamilyRequest();
        request.HasPartner = true;

        await FluentActions.Invoking(async () => await _sut.Execute(request))
            .Should().ThrowAsync<ValidationException>();

        _gatewayMock.Verify(x => x.CreateFosterFamily(It.IsAny<FosterFamilyRequest>()), Times.Never);
    }

    public static FosterFamilyRequest BuildValidFosterFamilyRequest()
    {
        return new FosterFamilyRequest
        {
            FosterCarer = new FosterCarerRequest
            {
                CarerFirstName = "Jane",
                CarerLastName = "Doe",
                CarerDateOfBirth = DateTime.Today.AddYears(-30),
                CarerNationalInsuranceNumber = "AA123456A"
            },
            FosterChild = new FosterChildRequest
            {
                ChildFirstName = "Child",
                ChildLastName = "Doe",
                ChildDateOfBirth = DateTime.Today.AddYears(-4),
                ChildPostCode = "SW1A 1AA"
            },
            SubmissionDate = DateTime.Today.AddDays(-1),
            HasPartner = false
        };
    }
}
