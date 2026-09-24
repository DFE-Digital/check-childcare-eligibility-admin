using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Gateways;

public class FosterFamiliesGateway : BaseGateway, IFosterFamiliesGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    private static readonly Dictionary<FosterFamiliesUrls, string> FosterFamiliesUrlsDict = new()
    {
        [FosterFamiliesUrls.FosterFamilySearch] = "foster-family/search",
        [FosterFamiliesUrls.FosterFamily] = "/foster-family/{fosterCarerId}",
        [FosterFamiliesUrls.FosterChild] = "/foster-family/child/{fosterChildId}",
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
            _logger.LogError(ex, $"Post CreateFosterFamily failed. uri:-{_httpClient.BaseAddress}foster-family");
            _logger.LogTrace(ex, $"Post CreateFosterFamily failed. uri:-{_httpClient.BaseAddress}foster-family content:-{JsonConvert.SerializeObject(request)}");
            throw;
        }
    }

    public async Task<FosterFamilyCodePreviewResponse> PreviewFosterFamilyCode(FosterFamilyRequest request, int localAuthorityId)
    {
        try
        {
            var result = await ApiDataPostAsynch("foster-family/preview", request, new FosterFamilyCodePreviewResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Post PreviewFosterFamilyCode failed. uri:-{_httpClient.BaseAddress}foster-family/preview");
            _logger.LogTrace(ex, $"Post PreviewFosterFamilyCode failed. uri:-{_httpClient.BaseAddress}foster-family/preview content:-{JsonConvert.SerializeObject(request)}");
            throw;
        }
    }

    public async Task<FosterFamilyResponse> GetFosterFamily(Guid fosterCarerId, int localAuthorityId, bool includeChildren = false)
    {
        try
        {
            var url = FosterFamiliesUrlsDict[FosterFamiliesUrls.FosterFamily].Replace("{fosterCarerId}", fosterCarerId.ToString());
            var response = await ApiDataGetAsynch($"{url}?includeChildren={includeChildren}", new FosterFamilyResponse());

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get GetFosterFamily failed. uri:-{_httpClient.BaseAddress}{FosterFamiliesUrlsDict[FosterFamiliesUrls.FosterFamily]}");
        }

        return null;
    }

    public async Task<FosterChildResponse> GetFosterChild(Guid fosterChildId, int localAuthorityId, bool includeFosterCarer = false)
    {
        try
        {
            var url = FosterFamiliesUrlsDict[FosterFamiliesUrls.FosterChild].Replace("{fosterChildId}", fosterChildId.ToString());
            var response = await ApiDataGetAsynch($"{url}?includeFosterCarer={includeFosterCarer}", new FosterChildResponse());

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get GetFosterChild failed. uri:-{_httpClient.BaseAddress}{FosterFamiliesUrlsDict[FosterFamiliesUrls.FosterChild]}");
        }

        return null;
    }

    public async Task UpdateFosterCarer(Guid fosterCarerId, int localAuthorityId, UpdateFosterCarerRequest request)
    {
        var url = FosterFamiliesUrlsDict[FosterFamiliesUrls.FosterFamily].Replace("{fosterCarerId}", fosterCarerId.ToString());
        try
        {
            _ = await ApiDataPatchAsynch(url, request, new object());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Patch UpdateFosterCarer failed. uri:-{_httpClient.BaseAddress}{url}");
            _logger.LogTrace(ex, $"Patch UpdateFosterCarer failed. uri:-{_httpClient.BaseAddress}{url} content:-{JsonConvert.SerializeObject(request)}");
            throw;
        }
    }

    public async Task UpdateFosterChild(Guid fosterChildId, int localAuthorityId, UpdateFosterChildRequest request)
    {
        var url = FosterFamiliesUrlsDict[FosterFamiliesUrls.FosterChild].Replace("{fosterChildId}", fosterChildId.ToString());
        try
        {
            _ = await ApiDataPatchAsynch(url, request, new object());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Patch UpdateFosterChild failed. uri:-{_httpClient.BaseAddress}{url}");
            _logger.LogTrace(ex, $"Patch UpdateFosterChild failed. uri:-{_httpClient.BaseAddress}{url} content:-{JsonConvert.SerializeObject(request)}");
            throw;
        }
    }
}
