namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestBulk
{
    public string? ClientIdentifier { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public IEnumerable<IEligibilityServiceType> Data { get; set; }
}