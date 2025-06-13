namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestBulk
{
    public string Filename { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public IEnumerable<CheckEligibilityRequestData> Data { get; set; }
}