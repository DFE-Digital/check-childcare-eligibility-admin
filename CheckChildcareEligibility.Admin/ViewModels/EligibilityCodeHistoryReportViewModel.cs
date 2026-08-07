using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.ViewModels;

public class EligibilityCodeHistoryReportViewModel
{
    public string EligibilityCode { get; set; }
    public WorkingFamilyEventByEligibilityCodeResponse Response { get; set; }
}