using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheckChildcareEligibility.Admin.Gateways.Tests;

internal class DerivedReportGateway : ReportGateway
{
    public DerivedReportGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(logger, httpClient, configuration, httpContextAccessor)
    {
        ApiErrorCount = 0;
    }
    public int ApiErrorCount { get; private set; }
    protected override Task LogApiErrorInternal(HttpResponseMessage task, string method, string uri, string data)
    {
        ApiErrorCount++;
        return Task.CompletedTask;
    }
    protected override Task LogApiErrorInternal(HttpResponseMessage task, string method, string uri)
    {
        ApiErrorCount++;
        return Task.CompletedTask;
    }
}