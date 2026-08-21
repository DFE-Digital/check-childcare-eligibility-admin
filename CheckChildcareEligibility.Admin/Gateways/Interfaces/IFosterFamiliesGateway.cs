using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface IFosterFamiliesGateway
{
    //FosterFamilies
    Task<FosterFamiliesSearchResponse> GetFosterFamiliesSearchRecords(int pageNumber, int pageSize);
    Task<FosterFamilyCreatedResponse> CreateFosterFamily(FosterFamilyRequest request);
    Task<FosterFamilyResponse> GetFosterFamily(Guid fosterCarerId, int localAuthorityId, bool includeChildren = false);
    Task<FosterChildResponse> GetFosterChild(Guid fosterChildId, int localAuthorityId, bool includeFosterCarer = false);
    Task UpdateFosterCarer(Guid fosterCarerId, int localAuthorityId, UpdateFosterCarerRequest request);
}