using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Helpers;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterCodeReconfirmCheckDetailsViewModel
    {
        public Guid FosterChildId { get; set; }

        public string ChildFullName { get; set; }

        public FosterCodeReconfirmDateViewModel FosterCodeReconfirmDateViewModel { get; set; } = new();

        public FosterCodePreviewResponse FosterCodePreview { get; set; } = new();

        public EligibilityCodeProperties CodeProperties { get; set; }

        public string CodeStatus => WorkingFamiliesCheckHelper.GetCodeStatus_FF(CodeProperties);
        
        public string GracePeriod => WorkingFamiliesCheckHelper.GetGracePeriod_FF(CodeProperties);

    }
}