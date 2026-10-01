using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.UseCases;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class LoadFosterChildDetailsUseCaseTests
{
    private LoadFosterChildDetailsUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new LoadFosterChildDetailsUseCase();
    }

    [Test]
    public async Task Execute_WhenResponseIsNull_ReturnsNull()
    {
        var result = await _sut.Execute();

        result.Should().BeNull();
    }

    [Test]
    public async Task Execute_WhenResponseIsProvided_MapsChildDetailsAndDateParts()
    {
        var dateOfBirth = new DateTime(2018, 11, 9);
        var response = new FosterChildResponse
        {
            FosterChildId = Guid.NewGuid(),
            ChildFirstName = "Child",
            ChildLastName = "Smith",
            ChildDateOfBirth = dateOfBirth,
            ChildPostCode = "SW1A 1AA"
        };

        var result = await _sut.Execute(response);

        result.Should().BeEquivalentTo(new
        {
            response.FosterChildId,
            response.ChildFirstName,
            response.ChildLastName,
            response.ChildDateOfBirth,
            response.ChildPostCode,
            Day = "9",
            Month = "11",
            Year = "2018"
        });
    }
}
