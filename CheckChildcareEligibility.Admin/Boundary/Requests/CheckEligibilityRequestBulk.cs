namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestBulk
{
    public IEnumerable<CheckEligibilityRequestData> Data { get; set; }
}