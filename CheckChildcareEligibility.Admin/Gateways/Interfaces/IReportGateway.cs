using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface IReportGateway
{
    Task<WorkingFamilyEventByEligibilityCodeResponse> GetAllWorkingFamiliesEventsByEligibilityCode(string eligibilityCode);
}