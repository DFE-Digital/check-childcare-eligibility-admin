using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface IFosterFamiliesGateway
{
    //FosterFamilies
    Task<FosterFamiliesSearchResponse> GetFosterFamiliesSearchRecords(int pageNumber, int pageSize);
    Task<FosterFamilyCreatedResponse> CreateFosterFamily(FosterFamilyRequest request);
    Task<FosterFamilyCodePreviewResponse> PreviewFosterFamilyCode(FosterFamilyRequest request, int localAuthorityId);
    Task<FosterFamilyResponse> GetFosterFamily(Guid fosterCarerId, int localAuthorityId, bool includeChildren = false);
    Task<FosterChildResponse> GetFosterChild(Guid fosterChildId, int localAuthorityId, bool includeFosterCarer = false);
    Task UpdateFosterCarer(Guid fosterCarerId, int localAuthorityId, UpdateFosterCarerRequest request);
}