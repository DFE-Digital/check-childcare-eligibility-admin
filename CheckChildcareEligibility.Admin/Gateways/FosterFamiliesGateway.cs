using Azure.Core;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing.Printing;

namespace CheckChildcareEligibility.Admin.Gateways;

public class FosterFamiliesGateway : BaseGateway, IFosterFamiliesGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    private static readonly Dictionary<FosterFamiliesUrls, string> FosterFamiliesUrlsDict = new()
    {
        [FosterFamiliesUrls.FosterFamilySearch] = "foster-family/search",
        [FosterFamiliesUrls.GetFosterFamily] = "/foster-family/{fosterCarerId}",
        [FosterFamiliesUrls.GetFosterChild] = "/foster-family/child/{fosterChildId}",
        [FosterFamiliesUrls.UpdateFosterCarer] = "/foster-family/{fosterCarerId}"
    };

    public FosterFamiliesGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base("EcsService",
        logger, httpClient, configuration, httpContextAccessor)
    {
        _logger = logger.CreateLogger("EcsService");
        _httpClient = httpClient;
    }

    public async Task<FosterFamiliesSearchResponse> GetFosterFamiliesSearchRecords(int pageNumber, int pageSize)
    {
        var url = FosterFamiliesUrlsDict[FosterFamiliesUrls.FosterFamilySearch];

        try
        {
            var response = await ApiDataGetAsynch($"{url}?pageNumber={pageNumber}&pageSize={pageSize}", new FosterFamiliesSearchResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get FosterFamiliesSearchRecords failed. uri:-{_httpClient.BaseAddress}{FosterFamiliesUrlsDict[FosterFamiliesUrls.FosterFamilySearch]}");
        }

        return null;
    }

    public async Task<FosterFamilyCreatedResponse> CreateFosterFamily(FosterFamilyRequest request)
    {
        try
        {
            var result = await ApiDataPostAsynch("foster-family", request, new FosterFamilyCreatedResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Post CreateFosterFamily failed. uri:-{_httpClient.BaseAddress}foster-family content:-{JsonConvert.SerializeObject(request)}");
            throw;
        }
    }

    public async Task<FosterFamilyResponse> GetFosterFamily(Guid fosterCarerId, int localAuthorityId, bool includeChildren = false)
    {
        try
        {
            var url = FosterFamiliesUrlsDict[FosterFamiliesUrls.GetFosterFamily].Replace("{fosterCarerId}", fosterCarerId.ToString());
            var response = await ApiDataGetAsynch($"{url}?includeChildren={includeChildren}", new FosterFamilyResponse());

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get GetFosterFamily failed. uri:-{_httpClient.BaseAddress}{FosterFamiliesUrlsDict[FosterFamiliesUrls.GetFosterFamily]}");
        }

        return null;
    }

    public async Task<FosterChildResponse> GetFosterChild(Guid fosterChildId, int localAuthorityId, bool includeFosterCarer = false)
    {
        try
        {
            var url = FosterFamiliesUrlsDict[FosterFamiliesUrls.GetFosterChild].Replace("{fosterChildId}", fosterChildId.ToString());
            var response = await ApiDataGetAsynch($"{url}?includeCarer={includeFosterCarer}", new FosterChildResponse());

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get GetFosterChild failed. uri:-{_httpClient.BaseAddress}{FosterFamiliesUrlsDict[FosterFamiliesUrls.GetFosterChild]}");
        }

        return null;
    }

    public async Task UpdateFosterCarer(Guid fosterCarerId, int localAuthorityId, UpdateFosterCarerRequest request)
    {
        var url = FosterFamiliesUrlsDict[FosterFamiliesUrls.UpdateFosterCarer].Replace("{fosterCarerId}", fosterCarerId.ToString());

        try
        {
            _ = await ApiDataPatchAsynch(url, request, new object());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Patch UpdateFosterCarer failed. uri:-{_httpClient.BaseAddress}{url} content:-{JsonConvert.SerializeObject(request)}");
            throw;
        }
    }
}