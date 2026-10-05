using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.UseCases;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class LoadFosterPartnerDetailsUseCaseTests
{
    private LoadFosterPartnerDetailsUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new LoadFosterPartnerDetailsUseCase();
    }

    [Test]
    public async Task Execute_WhenResponseIsNull_ReturnsNull()
    {
        var result = await _sut.Execute();

        result.Should().BeNull();
    }

    [Test]
    public async Task Execute_WhenResponseIsProvided_MapsPartnerDetailsAndDateParts()
    {
        var dateOfBirth = new DateTime(1988, 7, 3);
        var response = new FosterFamilyResponse
        {
            FosterCarerId = Guid.NewGuid(),
            PartnerFirstName = "John",
            PartnerLastName = "Smith",
            PartnerDateOfBirth = dateOfBirth,
            PartnerNationalInsuranceNumber = "AB123456D"
        };

        var result = await _sut.Execute(response);

        result.Should().BeEquivalentTo(new
        {
            response.FosterCarerId,
            response.PartnerFirstName,
            response.PartnerLastName,
            response.PartnerNationalInsuranceNumber,
            Day = "3",
            Month = "7",
            Year = "1988"
        });
    }
}
