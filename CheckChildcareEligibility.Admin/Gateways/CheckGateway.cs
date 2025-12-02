using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Gateways;

public class CheckGateway : BaseGateway, ICheckGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    private static readonly Dictionary<CheckEligibilityType, string> CheckUrls = new()
    {        
        [CheckEligibilityType.TwoYearOffer] = "check/two-year-offer",
        [CheckEligibilityType.EarlyYearPupilPremium] = "check/early-year-pupil-premium",
        [CheckEligibilityType.WorkingFamilies] = "check/working-families",
    };

    private static readonly Dictionary<CheckEligibilityType, string> BulkCheckUrls = new()
    {
        [CheckEligibilityType.TwoYearOffer] = "bulk-check/two-year-offer",
        [CheckEligibilityType.EarlyYearPupilPremium] = "bulk-check/early-year-pupil-premium",
        [CheckEligibilityType.WorkingFamilies] = "bulk-check/working-families",
    };

    public CheckGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base("EcsService",
        logger, httpClient, configuration, httpContextAccessor )
    {
        _logger = logger.CreateLogger("EcsService");
        _httpClient = httpClient;
    }

    private string CheckUrl(CheckEligibilityRequest checkEligibilityRequest)
    {
        if (checkEligibilityRequest.Data == null)
            return "error";

        return CheckUrls[checkEligibilityRequest.Data.Type];
    }

    private string BulkCheckUrl(CheckEligibilityRequestBulk checkEligibilityRequest)
    {
        if (checkEligibilityRequest.Data == null || !checkEligibilityRequest.Data.Any())
            return "error";        

        return BulkCheckUrls[checkEligibilityRequest.Data.First().Type];
    }

    public async Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest requestBody)
    {
        try
        {
            var checkUrl = CheckUrl(requestBody);

            var result = await ApiDataPostAsynch(checkUrl, requestBody, new CheckEligibilityResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Post Check failed. uri:-{_httpClient.BaseAddress}{CheckUrl(requestBody)} content:-{JsonConvert.SerializeObject(requestBody)}");
            throw;
        }
    }

    public async Task<CheckEligibilityItemWorkingFamiliesResponse> GetWFResult(string getEligibilityCheck)
    {
        try
        {
            var response = await ApiDataGetAsynch(getEligibilityCheck,
                new CheckEligibilityItemWorkingFamiliesResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get Status failed. uri:-{_httpClient.BaseAddress}{getEligibilityCheck}");
        }
        return null;
    }

    public async Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody)
    {
        try
        {
            var response = await ApiDataGetAsynch($"{responseBody.Links.Get_EligibilityCheck}/status",
                new CheckEligibilityStatusResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get Status failed. uri:-{_httpClient.BaseAddress}{responseBody.Links.Get_EligibilityCheck}/status");
        }

        return null;
    }

    public async Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string bulkCheckUrl)
    {
        try
        {
            var result = await ApiDataGetAsynch(bulkCheckUrl, new CheckEligibilityBulkStatusResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{bulkCheckUrl}");
        }

        return null;
    }

    public async Task<T> GetBulkCheckResults<T>(string resultsUrl) where T : CheckEligibilityBulkResponseBase, new()
    {
        try
        {
            var result = await ApiDataGetAsynch(resultsUrl, new T());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{resultsUrl}");
            throw;
        }
    }
    public async Task<IEnumerable<IBulkExport>> LoadBulkCheckResults(string bulkCheckId, CheckEligibilityType eligibilityType)
    {
        CheckEligibilityBulkResponseBase bulkResult;
      
        switch (eligibilityType)
        {
            case CheckEligibilityType.WorkingFamilies:
                bulkResult = await GetBulkCheckResults<CheckEligibilityBulkWorkingFamiliesResponse>($"bulk-check/{bulkCheckId}/");
                break;
            default:
                bulkResult = await GetBulkCheckResults<CheckEligibilityBulkResponse>($"bulk-check/{bulkCheckId}/");
                break;

        }
       return bulkResult.BulkDataMapper();
    }

    public async Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk requestBody)
    {
        try
        {
            var checkBulkUploadUrl = BulkCheckUrl(requestBody);

            var result =
                await ApiDataPostAsynch(checkBulkUploadUrl, requestBody, new CheckEligibilityResponseBulk());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Post failed. uri:-{_httpClient.BaseAddress}{BulkCheckUrl(requestBody)} content:-{JsonConvert.SerializeObject(requestBody)}");
            throw;
        }
    }

    public async Task<CheckEligibilityBulkProgressByLAResponse> GetBulkCheckStatuses(string organisationId)
    {
        try
        {
            var response = await ApiDataGetAsynch($"bulk-check/search?organisationId={organisationId}",
                new CheckEligibilityBulkProgressByLAResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get Status failed. uri:-{_httpClient.BaseAddress}bulk-check/search");
        }

        return null;
    }

    public async Task<CheckEligiblityBulkDeleteResponse> DeleteBulkChecksFor(string bulkCheckDeleteUrl)
    {
        try
        {
            var response = await ApiDataDeleteAsynch($"{bulkCheckDeleteUrl}", new CheckEligiblityBulkDeleteResponse());
 
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Delete Bulk failed. uri:-{_httpClient.BaseAddress}bulk-check");
        }

        return null;
    }
}