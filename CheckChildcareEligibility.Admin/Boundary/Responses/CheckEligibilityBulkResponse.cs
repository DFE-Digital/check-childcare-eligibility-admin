using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Models;
using static CheckChildcareEligibility.Admin.Models.BulkExportBase;

namespace CheckChildcareEligibility.Admin.Boundary.Responses;

public class CheckEligibilityBulkResponse : CheckEligibilityBulkResponseBase
{
    public IEnumerable<CheckEligibilityItem> Data { get; set; }

    public override IEnumerable<IBulkExport> BulkDataMapper(CheckEligibilityType eligibilityType)
    {
        var orderedData = Data.OrderBy(x => x.Order);

        return eligibilityType switch
        {
            CheckEligibilityType.EarlyYearPupilPremium => orderedData.Select(x => new BulkExportEYPP
            {
                LastName = x.LastName,
                Reference = x.Reference?.Trim(),
                DOB = x.DateOfBirth,
                NI = x.NationalInsuranceNumber,
                NASS = x.NationalAsylumSeekerServiceNumber,
                Outcome = GetStatusDescription(x.Status),
            }),
            _ => orderedData.Select(x => new BulkExport2YO
            {
                LastName = x.LastName,
                Reference = x.Reference?.Trim(),
                DOB = x.DateOfBirth,
                NI = x.NationalInsuranceNumber,
                Outcome = GetStatusDescription(x.Status),
            })
        };
    }

    private string GetStatusDescription(string status)
    {
        Enum.TryParse(status, out CheckEligibilityStatus statusEnum);

        switch (statusEnum)
        {
            case CheckEligibilityStatus.parentNotFound:
                return "Information does not match records";

            case CheckEligibilityStatus.eligible:
                return "Eligible";

            case CheckEligibilityStatus.notEligible:
                return "May not be eligible";

            case CheckEligibilityStatus.error:
                return "Try again";

            default:
                return status;
        }
    }
}

public abstract class CheckEligibilityBulkResponseBase
{
    public abstract IEnumerable<IBulkExport> BulkDataMapper(CheckEligibilityType eligibilityType);
}

public class CheckEligibilityBulkWorkingFamiliesResponse : CheckEligibilityBulkResponseBase
{
    public IEnumerable<CheckEligibilityItemWorkingFamilies> Data { get; set; }

    public override IEnumerable<IBulkExport> BulkDataMapper(CheckEligibilityType eligibilityType)
    {
        return Data.OrderBy(x => x.Order).Select(x => new BulkExportWorkingFamilies
        {
            EligibilityCode = x.EligibilityCode,
            Reference = x.Reference?.Trim(),
            ChildDOB = x.DateOfBirth,
            NI = x.NationalInsuranceNumber,
            ValidityStartDate = x.ValidityStartDate.ToString("yyyy-MM-dd"),
            GracePeriodEnds = x.GracePeriodEndDate.ToString("yyyy-MM-dd"),
            ValidityEndDate = x.ValidityEndDate.ToString("yyyy-MM-dd"),
            Outcome = GetStatusDescription(x.Status),
        });
    }

    private string GetStatusDescription(string status)
    {
        Enum.TryParse(status, out CheckEligibilityStatus statusEnum);

        switch (statusEnum)
        {
            case CheckEligibilityStatus.notFound:
                return "Information does not match records";

            case CheckEligibilityStatus.eligible:
                return "Code valid";

            case CheckEligibilityStatus.notEligible:
                return "Code expired";

            case CheckEligibilityStatus.error:
                return "System error - try again later";

            default:
                return status;
        }
    }
}