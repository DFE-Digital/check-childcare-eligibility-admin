using CheckChildcareEligibility.Admin.Domain.Enums;
using System.Security.AccessControl;

namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public interface IEligibilityServiceType
{
    public CheckEligibilityType Type { get; set; }
}
