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
public class ValidateFosterPartnerDetailsUseCaseTests
{
    private Mock<IFosterFamiliesGateway> _gatewayMock;
    private Mock<ILogger<ValidateFosterPartnerDetailsUseCase>> _loggerMock;
    private ValidateFosterPartnerDetailsUseCase _sut;

    [SetUp]
    public void SetUp()
    {
        _gatewayMock = new Mock<IFosterFamiliesGateway>();
        _loggerMock = new Mock<ILogger<ValidateFosterPartnerDetailsUseCase>>();
        _sut = new ValidateFosterPartnerDetailsUseCase(_loggerMock.Object, _gatewayMock.Object);
    }

    [Test]
    public async Task Execute_WhenDuplicatePartnerNinoExists_ShouldReturnConflictDetails()
    {
        var conflictingCarerId = Guid.NewGuid();
        var request = new FosterPartnerDetailsViewModel
        {
            PartnerFirstName = "John",
            PartnerLastName = "Smith",
            Day = "12",
            Month = "04",
            Year = "1990",
            PartnerNationalInsuranceNumber = "AB123456C",
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
        result.Errors.Should().ContainKey("PartnerNationalInsuranceNumber");
        result.Errors["PartnerNationalInsuranceNumber"].Should().Contain(ValidationMessages.PartnerAlreadyExists);
        modelState.Should().ContainKey("PartnerNationalInsuranceNumber");
    }

    [Test]
    public async Task Execute_WhenNoDuplicatePartnerNinoExists_ShouldReturnValidResult()
    {
        var request = new FosterPartnerDetailsViewModel
        {
            PartnerFirstName = "John",
            PartnerLastName = "Smith",
            Day = "12",
            Month = "04",
            Year = "1990",
            PartnerNationalInsuranceNumber = "AB123456C",
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
