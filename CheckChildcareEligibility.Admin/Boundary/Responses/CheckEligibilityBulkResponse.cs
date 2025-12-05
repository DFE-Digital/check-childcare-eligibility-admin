using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.Boundary.Responses;

public abstract class CheckEligibilityBulkResponseBase {

    public abstract IEnumerable<IBulkExport> BulkDataMapper();
}
public class CheckEligibilityBulkResponse : CheckEligibilityBulkResponseBase
{
    public IEnumerable<CheckEligibilityItem> Data { get; set; }

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
    public override IEnumerable<IBulkExport> BulkDataMapper() {

        return Data.Select(x => new BulkExport
        {
            LastName = x.LastName,
            DOB = x.DateOfBirth,
            NI = x.NationalInsuranceNumber,
            Outcome = GetStatusDescription(x.Status),
        });
    }

}
public class CheckEligibilityBulkWorkingFamiliesResponse : CheckEligibilityBulkResponseBase
{
    public IEnumerable<CheckEligibilityItemWorkingFamilies> Data { get; set; }

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
    public override IEnumerable<IBulkExport> BulkDataMapper()
    {

        return Data.Select(x => new BulkExportWorkingFamilies
        {
            EligibilityCode = x.EligibilityCode,
            ChildDOB = x.DateOfBirth,
            NI = x.NationalInsuranceNumber,
            ValidityStartDate = x.ValidityStartDate,
            GracePeriodEnds = x.GracePeriodEndDate,
            ValidityEndDate = x.ValidityEndDate,
            Outcome = GetStatusDescription(x.Status),

        });
    }
}