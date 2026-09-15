using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Models;
using FluentAssertions;
using NUnit.Framework;

namespace CheckChildcareEligibility.Admin.Tests.Boundary.Responses;

[TestFixture]
public class CheckEligibilityBulkResponseTests
{
    [Test]
    public void BulkDataMapper_StandardResults_ReturnsRowsInUploadedOrder()
    {
        var response = new CheckEligibilityBulkResponse
        {
            Data =
            [
                new CheckEligibilityItem
                {
                    Order = 2,
                    LastName = "SECOND",
                    Status = "eligible"
                },
                new CheckEligibilityItem
                {
                    Order = 1,
                    LastName = "FIRST",
                    Status = "eligible"
                }
            ]
        };

        var result = response.BulkDataMapper()
            .Cast<BulkExport>()
            .ToList();

        result.Select(x => x.LastName)
            .Should()
            .Equal("FIRST", "SECOND");
    }

    [Test]
    public void BulkDataMapper_WorkingFamiliesResults_ReturnsRowsInUploadedOrder()
    {
        var response = new CheckEligibilityBulkWorkingFamiliesResponse
        {
            Data =
            [
                new CheckEligibilityItemWorkingFamilies
                {
                    Order = 2,
                    EligibilityCode = "SECOND",
                    Status = "eligible"
                },
                new CheckEligibilityItemWorkingFamilies
                {
                    Order = 1,
                    EligibilityCode = "FIRST",
                    Status = "eligible"
                }
            ]
        };

        var result = response.BulkDataMapper()
            .Cast<BulkExportWorkingFamilies>()
            .ToList();

        result.Select(x => x.EligibilityCode)
            .Should()
            .Equal("FIRST", "SECOND");
    }

    [Test]
    public void BulkDataMapper_WorkingFamiliesResults_FormatsDatesAsYearMonthDay()
    {
        var response = new CheckEligibilityBulkWorkingFamiliesResponse
        {
            Data =
            [
                new CheckEligibilityItemWorkingFamilies
                {
                    ValidityStartDate = new DateTime(2027, 7, 1),
                    ValidityEndDate = new DateTime(2027, 9, 30),
                    GracePeriodEndDate = new DateTime(2027, 11, 30)
                }
            ]
        };

        var result = response.BulkDataMapper()
            .Cast<BulkExportWorkingFamilies>()
            .Single();

        result.ValidityStartDate.Should().Be("2027-07-01");
        result.ValidityEndDate.Should().Be("2027-09-30");
        result.GracePeriodEnds.Should().Be("2027-11-30");
    }
}