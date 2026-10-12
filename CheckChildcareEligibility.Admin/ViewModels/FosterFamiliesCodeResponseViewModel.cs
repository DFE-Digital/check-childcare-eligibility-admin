using System.Text.Json.Serialization;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Helpers;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels;

public class FosterFamiliesCodeResponseViewModel
{
    public FosterChildResponse Response { get; set; }

    public EligibilityCodeProperties CodeProperties { get; set; }

    [JsonIgnore]
    public string CodeStatus => WorkingFamiliesCheckHelper.GetCodeStatus_FF(CodeProperties);

    [JsonIgnore]
    public string GracePeriod => WorkingFamiliesCheckHelper.GetGracePeriod_FF(CodeProperties);

    [JsonIgnore]
    public string[] ReconfirmStatus => WorkingFamiliesCheckHelper.GetReconfirmStatus(CodeProperties.ReconfirmationProperties);

    public string? Confirmation { get; set; }

}