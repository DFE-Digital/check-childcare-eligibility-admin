using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface IFosterFamiliesGateway
{
    Task<FosterFamiliesSearchResponse> GetFosterFamiliesSearchRecords(int pageNumber, int pageSize, string ninoFilter = "");

    Task<FosterFamilyCreatedResponse> CreateFosterFamily(FosterFamilyRequest request);

    Task<FosterCodePreviewResponse> PreviewFosterFamilyCode(FosterFamilyRequest request);

    Task<FosterFamilyResponse> GetFosterFamily(Guid fosterCarerId, bool includeChildren = false);

    Task<FosterChildResponse> GetFosterChild(Guid fosterChildId, bool includeFosterCarer = false);

    Task UpdateFosterCarer(Guid fosterCarerId, UpdateFosterCarerRequest request);

    Task UpdateFosterChild(Guid fosterChildId, UpdateFosterChildRequest request);

    Task<FosterCodePreviewResponse> PreviewFosterChildReconfirm(Guid fosterChildId, FosterChildReconfirmRequest request);

    Task<FosterChildResponse> ReconfirmFosterChild(Guid fosterChildId, FosterChildReconfirmRequest request);

}