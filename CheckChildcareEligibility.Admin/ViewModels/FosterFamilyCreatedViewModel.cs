
using System.Text.Json.Serialization;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Helpers;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterFamilyCreatedViewModel
    {

        public FosterChildResponse Response { get; init; }

        public bool IsAdditionalChild { get; init; }

        [JsonIgnore]
        public string CodeStatus => WorkingFamiliesCheckHelper.GetCodeStatus_FF(new EligibilityCodeProperties(Response));

        [JsonIgnore]
        public string GracePeriod => WorkingFamiliesCheckHelper.GetGracePeriod_FF(new EligibilityCodeProperties(Response));
    }
}
