using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Boundary.Responses;

public class PostCheckResult
{
    public string Id { get; set; }
    public CheckEligibilityStatus Status { get; set; }
}