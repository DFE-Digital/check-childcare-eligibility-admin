using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestDataBase : IEligibilityServiceType
{
    public CheckEligibilityType Type { get; set; }
    public int? Sequence { get; set; }
    public string DateOfBirth { get; set; } = string.Empty;
    public string NationalInsuranceNumber { get; set; } = string.Empty;
}