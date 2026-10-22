using System.Text;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Usecases;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace CheckChildcareEligibility.Admin.Tests.Usecases;

[TestFixture]
public class BulkDateFormatTests
{
    private static readonly CheckEligibilityType[] Types =
    [
        CheckEligibilityType.TwoYearOffer,
        CheckEligibilityType.EarlyYearPupilPremium,
        CheckEligibilityType.WorkingFamilies
    ];

    [Test]
    public async Task SupportedDates_AreConvertedToIsoBeforeSendingToApi(
        [ValueSource(nameof(Types))] CheckEligibilityType type,
        [Values("2022-02-13", "13-02-2022", "13/02/2022", "1/2/2022", "02/03/2022")] string date)
    {
        var result = await Parse(type, date);

        result.ErrorMessage.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
        result.ValidRequests.Should().ContainSingle().Which.DateOfBirth.Should().Be(
            date switch
            {
                "1/2/2022" => "2022-02-01",
                "02/03/2022" => "2022-03-02",
                _ => "2022-02-13"
            });
    }

    [Test]
    public async Task InvalidDates_ReportTheDateErrorForTheCsvRow(
        [ValueSource(nameof(Types))] CheckEligibilityType type,
        [Values("31/02/2022", "29/02/2023", "2022/02/13", "13/02/22", "13//2022")] string date)
    {
        var result = await Parse(type, date);

        result.ErrorMessage.Should().BeEmpty();
        result.ValidRequests.Should().BeEmpty();
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new CsvRowError
        {
            LineNumber = 2,
            Message = ValidationMessages.ValidDOB
        });
    }

    private static async Task<BulkCheckCsvResult> Parse(CheckEligibilityType type, string date)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["BulkEligibilityCheckLimit"] = "250" })
            .Build();
        var parser = new ParseBulkCheckFileUseCase(new CheckEligibilityRequestDataValidator(), config);
        var csv = type == CheckEligibilityType.WorkingFamilies
            ? $"Eligibility code,National Insurance number,Child date of birth,Reference (optional)\r\n50173110190,AB123456C,{date},\r\n"
            : $"Parent Last Name,Parent Date of Birth,Parent National Insurance number,Reference (optional)\r\nSmith,{date},AB123456C,\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        return await parser.Execute(stream, type);
    }
}
