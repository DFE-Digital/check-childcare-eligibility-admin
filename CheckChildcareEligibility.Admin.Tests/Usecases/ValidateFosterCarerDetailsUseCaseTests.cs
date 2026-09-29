using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ValidateFosterCarerDetailsUseCaseTests
{
    private Mock<IFosterFamiliesGateway> _gatewayMock;
    private Mock<ILogger<ValidateFosterCarerDetailsUseCase>> _loggerMock;
    private ValidateFosterCarerDetailsUseCase _sut;

    [SetUp]
    public void SetUp()
    {
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _loggerMock = new Mock<ILogger<ValidateFosterCarerDetailsUseCase>>();
        _sut = new ValidateFosterCarerDetailsUseCase(_loggerMock.Object, _gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenDuplicateNinoExists_ShouldReturnConflictDetails()
    {
        var conflictingCarerId = Guid.NewGuid();
        var request = new FosterCarerDetailsViewModel
        {
            CarerFirstName = "Jane",
            CarerLastName = "Smith",
            Day = "12",
            Month = "04",
            Year = "1990",
            CarerNationalInsuranceNumber = "AB123456C",
            HasPartner = false,
            FosterCarerId = Guid.Empty
        };
        var modelState = new ModelStateDictionary();

        _gatewayMock
            .Setup(x => x.GetFosterFamiliesSearchRecords(1, 2, "AB123456C"))
            .ReturnsAsync(new FosterFamiliesSearchResponse
            {
                Data =
                [
                    new FosterFamiliesSearchItemResponse { FosterCarerId = conflictingCarerId }
                ]
            });

        var result = await _sut.Execute(request, modelState);

        result.IsValid.Should().BeFalse();
        result.ConflictingFosterCarerId.Should().Be(conflictingCarerId);
        result.Errors.Should().ContainKey("CarerNationalInsuranceNumber");
        result.Errors["CarerNationalInsuranceNumber"].Should().Contain(ValidationMessages.CarerAlreadyExists);
        modelState.Should().ContainKey("CarerNationalInsuranceNumber");
    }

    [Test]
    public async Task Execute_WhenNoDuplicateNinoExists_ShouldReturnValidResult()
    {
        var request = new FosterCarerDetailsViewModel
        {
            CarerFirstName = "Jane",
            CarerLastName = "Smith",
            Day = "12",
            Month = "04",
            Year = "1990",
            CarerNationalInsuranceNumber = "AB123456C",
            HasPartner = false,
            FosterCarerId = Guid.Empty
        };
        var modelState = new ModelStateDictionary();

        _gatewayMock
            .Setup(x => x.GetFosterFamiliesSearchRecords(1, 2, "AB123456C"))
            .ReturnsAsync(new FosterFamiliesSearchResponse { Data = [] });

        var result = await _sut.Execute(request, modelState);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeNull();
        result.ConflictingFosterCarerId.Should().Be(Guid.Empty);
    }
}
