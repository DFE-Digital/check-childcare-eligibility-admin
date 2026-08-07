using CheckChildcareEligibility.Admin.Boundary.Responses;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckChildcareEligibility.Admin.Models
{
    public class FosterFamily
    {
        public Guid FosterCarerId { get; set; }

        public string CarerFirstName { get; set; }
        public string CarerLastName { get; set; }
        
        [NotMapped]
        [Dob("date of birth", "carer", null, "Day", "Month", "Year")]
        public DateTime CarerDateOfBirth { get; set; }
        public string? Day { get; set; }
        public string? Month { get; set; }
        public string? Year { get; set; }

        public string CarerNationalInsuranceNumber { get; set; }

        public bool HasPartner { get; set; }

        public string? PartnerFirstName { get; set; }
        public string? PartnerLastName { get; set; }
        public DateTime? PartnerDateOfBirth { get; set; }
        public string? PartnerNationalInsuranceNumber { get; set; }

        public DateTime SubmissionDate { get; set; }

        // Populated when includeChildren = true
        public List<FosterChildSummaryResponse> FosterChildren { get; set; } = [];
    }
}
