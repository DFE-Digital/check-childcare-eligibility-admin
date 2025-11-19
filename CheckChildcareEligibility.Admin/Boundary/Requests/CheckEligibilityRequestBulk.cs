namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestBulk
{
    public IEnumerable<CheckEligibilityRequestData> Data { get; set; }
    public CheckEligibilityRequestBulkMeta Meta { get; set; }
}

public class CheckEligibilityRequestBulkMeta
{
    public string Filename { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public IEnumerable<IEligibilityServiceType> Data { get; set; }
}