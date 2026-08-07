using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;

namespace CheckChildcareEligibility.Admin.Gateways;

public class ReportGateway : BaseGateway, IReportGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public ReportGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base("EcsService",
        logger, httpClient, configuration, httpContextAccessor )
    {
        _logger = logger.CreateLogger("EcsService");
        _httpClient = httpClient;
    }

    public async Task<WorkingFamilyEventByEligibilityCodeResponse> GetAllWorkingFamiliesEventsByEligibilityCode(string eligibilityCode)
    {
        var uri = $"/working-families-reporting/{eligibilityCode}";

        try
        {
            var response = await ApiDataGetAsynch(uri, new WorkingFamilyEventByEligibilityCodeResponse());

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
            ex,
            $"GetAllWorkingFamiliesEventsByEligibilityCode failed. uri:-{_httpClient.BaseAddress}{uri}");
        }
        return null;
    }
}