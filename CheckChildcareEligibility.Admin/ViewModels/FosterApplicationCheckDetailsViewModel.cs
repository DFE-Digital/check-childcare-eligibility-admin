using System.Text.Json.Serialization;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Helpers;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterApplicationCheckDetailsViewModel
    {
        public string? ContextId { get; set; }

        public Guid FosterCarerId { get; set; }

        public bool IsAdditionalChild { get; set; }

        public FosterCarerDetailsViewModel FosterCarerDetailsViewModel { get; set; } = new();

        public FosterPartnerDetailsViewModel? FosterPartnerDetailsViewModel { get; set; }

        public FosterChildDetailsViewModel FosterChildDetailsViewModel { get; set; } = new();

        public FosterApplicationSubmittedDateViewModel FosterApplicationSubmittedDateViewModel { get; set; } = new();

        public FosterCodePreviewResponse FosterCodePreview { get; set; } = new();

        public EligibilityCodeProperties CodeProperties { get; set; }

        [JsonIgnore]
        public string CodeStatus => WorkingFamiliesCheckHelper.GetCodeStatus_FF(CodeProperties);
        
    }
}