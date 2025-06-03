using System.Security.Cryptography.Xml;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Gateways;

public class CheckGateway : BaseGateway, ICheckGateway
{
    private readonly string _checkBulkUploadUrl;
    private readonly string _checkUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    private static readonly Dictionary<string, string> CheckUrls = new()
    {
        ["FreeSchoolMeals"] = "check/free-school-meals",
        ["TwoYearOffer"] = "check/two-year-offer",
        ["EarlyYearPupilPremium"] = "check/early-year-pupil-premium",
    };

    private static readonly Dictionary<string, string> BulkCheckUrls = new()
    {
        ["FreeSchoolMeals"] = "bulk-check/free-school-meals",
        ["TwoYearOffer"] = "bulk-check/two-year-offer",
        ["EarlyYearPupilPremium"] = "bulk-check/early-year-pupil-premium",
    };

    public CheckGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration) : base("EcsService",
        logger, httpClient, configuration)
    {
        _logger = logger.CreateLogger("EcsService");
        _httpClient = httpClient;
    }

    private string CheckUrl(CheckEligibilityRequest checkEligibilityRequest)
    {
        if (checkEligibilityRequest.Data == null)
            return "error";

        return CheckUrls[checkEligibilityRequest.Data.CheckType];
    }

    private string BulkCheckUrl(CheckEligibilityRequestBulk checkEligibilityRequest)
    {
        if (checkEligibilityRequest.Data == null || !checkEligibilityRequest.Data.Any())
            return "error";        

        return BulkCheckUrls[checkEligibilityRequest.Data.First().CheckType];
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

    public async Task<CheckEligibilityBulkResponse> GetBulkCheckResults(string resultsUrl)
    {
        try
        {
            var result = await ApiDataGetAsynch(resultsUrl, new CheckEligibilityBulkResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{resultsUrl}");
            throw;
        }
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

    private static CheckEligibilityType ExtractCheckType(CheckEligibilityRequestData data)
    {
        CheckEligibilityType checkType = CheckEligibilityType.None;

        switch (data.CheckType)
        {
            case "FreeSchoolMeals":
                checkType = CheckEligibilityType.FreeSchoolMeals;
                break;
            case "TwoYearOffer":
                checkType = CheckEligibilityType.TwoYearOffer;
                break;
            case "EarlyYearPupilPremium":
                checkType = CheckEligibilityType.EarlyYearPupilPremium;
                break;
        }

        return checkType;
    }
}