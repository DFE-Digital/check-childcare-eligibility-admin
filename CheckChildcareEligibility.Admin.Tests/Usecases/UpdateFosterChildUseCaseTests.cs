using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Usecases;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class UpdateFosterChildUseCaseTests
{
    private Mock<IFosterFamiliesGateway> _gatewayMock = null!;
    private UpdateFosterChildUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _sut = new UpdateFosterChildUseCase(_gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenFosterChildIdIsEmpty_ThrowsValidationException()
    {
        var request = new UpdateFosterChildRequest
        {
            FosterChildRequest = new FosterChildRequest
            {
                ChildFirstName = "Child",
                ChildLastName = "Doe",
                ChildDateOfBirth = DateTime.Today.AddYears(-4),
                ChildPostCode = "SW1A 1AA"
            }
        };

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
    public async Task Execute_WhenChildRequestIsInvalid_ThrowsValidationException()
    {
        var request = new UpdateFosterChildRequest
        {
            FosterChildRequest = new FosterChildRequest
            {
                ChildFirstName = "",
                ChildLastName = "Doe",
                ChildDateOfBirth = DateTime.Today.AddYears(-4),
                ChildPostCode = "SW1A 1AA"
            }
        };

        await FluentActions.Invoking(async () => await _sut.Execute(Guid.NewGuid(), 123, request))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Execute_WhenRequestIsValid_CallsGateway()
    {
        var id = Guid.NewGuid();
        var request = new UpdateFosterChildRequest
        {
            FosterChildRequest = new FosterChildRequest
            {
                ChildFirstName = "Child",
                ChildLastName = "Doe",
                ChildDateOfBirth = DateTime.Today.AddYears(-4),
                ChildPostCode = "SW1A 1AA"
            }
        };

        await _sut.Execute(id, 456, request);

        _gatewayMock.Verify(x => x.UpdateFosterChild(id, 456, request), Times.Once);
    }
}
