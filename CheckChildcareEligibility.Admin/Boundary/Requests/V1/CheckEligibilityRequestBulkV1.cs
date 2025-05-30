namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestBulkV1
{
    public IEnumerable<CheckEligibilityRequestDataV1> Data { get; set; }
}