using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.UseCases;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class LoadFosterCarerDetailsUseCaseTests
{
    private LoadFosterCarerDetailsUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new LoadFosterCarerDetailsUseCase();
    }

    [Test]
    public async Task Execute_WhenResponseIsNull_ReturnsNull()
    {
        var result = await _sut.Execute();

        result.Should().BeNull();
    }

    [Test]
    public async Task Execute_WhenResponseIsProvided_MapsCarerDetailsAndDateParts()
    {
        var dateOfBirth = new DateTime(1990, 4, 12);
        var response = new FosterFamilyResponse
        {
            FosterCarerId = Guid.NewGuid(),
            CarerFirstName = "Jane",
            CarerLastName = "Smith",
            CarerDateOfBirth = dateOfBirth,
            CarerNationalInsuranceNumber = "AB123456C",
            HasPartner = true
        };

        var result = await _sut.Execute(response);

        result.Should().BeEquivalentTo(new
        {
            response.FosterCarerId,
            response.CarerFirstName,
            response.CarerLastName,
            response.CarerDateOfBirth,
            response.CarerNationalInsuranceNumber,
            response.HasPartner,
            Day = "12",
            Month = "4",
            Year = "1990"
        });
    }
}
