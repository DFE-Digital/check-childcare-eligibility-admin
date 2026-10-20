
using System.Text.Json.Serialization;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Helpers;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterCodeReconfirmedViewModel
    {

        public FosterChildResponse Response { get; init; }

        public EligibilityCodeProperties CodeProperties { get; init; }

        [JsonIgnore]
        public string CodeStatus => WorkingFamiliesCheckHelper.GetCodeStatus_FF(CodeProperties);
        
    }
}
