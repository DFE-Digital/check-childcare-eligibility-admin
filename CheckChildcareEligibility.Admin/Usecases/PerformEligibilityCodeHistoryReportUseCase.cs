using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface IPerformEligibilityCodeHistoryReportUseCase
{
    Task<WorkingFamilyEventByEligibilityCodeResponse> Execute(string eligibilityCode);
}

public class PerformEligibilityCodeHistoryReportUseCase : IPerformEligibilityCodeHistoryReportUseCase
{
    private readonly IReportGateway _reportGateway;
    private readonly ILogger<PerformEligibilityCodeHistoryReportUseCase> _logger;

    public PerformEligibilityCodeHistoryReportUseCase(
    ILogger<PerformEligibilityCodeHistoryReportUseCase> logger,
    IReportGateway reportGateway)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reportGateway = reportGateway ?? throw new ArgumentNullException(nameof(reportGateway));
    }

    public async Task<WorkingFamilyEventByEligibilityCodeResponse> Execute(string eligibilityCode)
    {
        if (string.IsNullOrWhiteSpace(eligibilityCode))
        {
            _logger.LogWarning("No eligibility code supplied.");
            throw new ArgumentException("Eligibility code is required.", nameof(eligibilityCode));
        }

        var response = await _reportGateway.GetAllWorkingFamiliesEventsByEligibilityCode(eligibilityCode);
        return response;
    }
}