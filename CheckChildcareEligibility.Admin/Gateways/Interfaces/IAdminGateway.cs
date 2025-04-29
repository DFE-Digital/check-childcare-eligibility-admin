using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface IAdminGateway
{
    Task<ApplicationItemResponse> GetApplication(string id);
    Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch requestBody);
    Task<ApplicationStatusUpdateResponse> PatchApplicationStatus(string id, ApplicationStatus status);
}