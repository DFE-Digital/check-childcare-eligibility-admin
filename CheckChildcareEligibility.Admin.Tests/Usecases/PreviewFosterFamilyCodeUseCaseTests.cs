using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Usecases;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class PreviewFosterFamilyCodeUseCaseTests
{
    private Mock<ILogger<PreviewFosterFamilyCodeUseCase>> _loggerMock = null!;
    private Mock<IFosterFamiliesGateway> _gatewayMock = null!;
    private PreviewFosterFamilyCodeUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<PreviewFosterFamilyCodeUseCase>>();
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _sut = new PreviewFosterFamilyCodeUseCase(_loggerMock.Object, _gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        await FluentActions.Invoking(async () => await _sut.Execute(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsInvalid_ThrowsValidationException()
    {
        var request = new FosterFamilyRequest
        {
            FosterCarer = new FosterCarerRequest
            {
                CarerFirstName = "Jane",
                CarerLastName = "",
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
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsValid_ReturnsPreviewResponse()
    {
        var request = CreateFosterFamilyUseCaseTests.BuildValidFosterFamilyRequest();
        var expected = new FosterCodePreviewResponse
        {
            ValidityStartDate = DateTime.Today.AddDays(-5),
            ValidFromTerm = new Term(TermName.Spring, DateTime.Today.AddDays(-10)),
            ReconfirmBetweenStart = DateTime.Today.AddDays(-2),
            ReconfirmBetweenEnd = DateTime.Today.AddDays(2),
            GracePeriodEndDate = DateTime.Today.AddDays(30)
        };

        _gatewayMock.Setup(x => x.PreviewFosterFamilyCode(request)).ReturnsAsync(expected);

        var result = await _sut.Execute(request);

        result.Should().BeEquivalentTo(expected);
        request.FosterCarer.LocalAuthorityID.Should().Be(456);
        _gatewayMock.Verify(x => x.PreviewFosterFamilyCode(request), Times.Once);
    }
}
