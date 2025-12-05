using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public interface IEligibilityServiceType
{
    public CheckEligibilityType Type { get; set; }
}