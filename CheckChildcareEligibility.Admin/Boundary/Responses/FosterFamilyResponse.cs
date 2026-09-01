namespace CheckChildcareEligibility.Admin.Boundary.Responses;

public class FosterFamilyResponse
{
    public Guid FosterCarerId { get; set; }

    public string CarerFirstName { get; set; }
    public string CarerLastName { get; set; }
    public DateTime CarerDateOfBirth { get; set; }
    public string CarerNationalInsuranceNumber { get; set; }

    public bool HasPartner { get; set; }

    public string? PartnerFirstName { get; set; }
    public string? PartnerLastName { get; set; }
    public DateTime? PartnerDateOfBirth { get; set; }
    public string? PartnerNationalInsuranceNumber { get; set; }

    public DateTime SubmissionDate { get; set; }

    // Populated when includeChildren = true
    public List<FosterChildSummaryResponse> FosterChildren { get; set; } = [];
}

public class FosterChildSummaryResponse
{
    public Guid FosterChildId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string EligibilityCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
