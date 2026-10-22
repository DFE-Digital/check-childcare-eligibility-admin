using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestDataBase : IEligibilityServiceType
{
    private string? reference;

    public string? Reference
    {
        get => reference;
        set => reference = value?.Trim();
    }

    public CheckEligibilityType Type { get; set; }
    public int? Order { get; set; }
    public string DateOfBirth { get; set; } = string.Empty;
    public string NationalInsuranceNumber { get; set; } = string.Empty;
}