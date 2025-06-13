namespace CheckChildcareEligibility.Admin.Boundary.Responses;

public class CheckEligibilityResponse
{
    public StatusValue Data { get; set; }
    public CheckEligibilityResponseLinks Links { get; set; }
}

public class CheckEligibilityResponseBulk
{
    public StatusValue Data { get; set; }
    public CheckEligibilityResponseBulkLinks Links { get; set; }
}

public class CheckEligibilityResponseBulkLinks
{
    public string Get_Progress_Check { get; set; }
    public string Get_BulkCheck_Results { get; set; }
}

public class CheckEligibilityBulkProgressResponse
{
    public string Guid { get; set; }
    public string ClientIdentifier { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string EligibilityType { get; set; } = string.Empty;
    public string DateSubmitted { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

}

public class CheckEligibilityBulkProgressByLAResponse
{
    public IEnumerable<CheckEligibilityBulkProgressResponse> Results { get; set; }
    
}
