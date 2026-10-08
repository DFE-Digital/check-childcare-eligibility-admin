using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Helpers;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels;

public class FosterFamiliesCodeResponseViewModel
{
    public FosterChildResponse Response { get; set; }

    public EligibilityCodeProperties CodeProperties { get; set; }

    public string CodeStatus => WorkingFamiliesCheckHelper.GetCodeStatus_FF(CodeProperties);

    public string GracePeriod => WorkingFamiliesCheckHelper.GetGracePeriod_FF(CodeProperties);

    public string[] ReconfirmStatus => WorkingFamiliesCheckHelper.GetReconfirmStatus(CodeProperties.ReconfirmationProperties);

    public string? Confirmation { get; set; }

}