using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestDataBase : IEligibilityServiceType
{
    protected CheckEligibilityType baseType;
    public int? Sequence { get; set; }

    public string CheckType => baseType.ToString();
}