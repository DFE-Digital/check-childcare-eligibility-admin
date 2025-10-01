using System.Net;

namespace CheckChildcareEligibility.Admin.Middleware;

public class CustomExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

    public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException occurred: {Message}", ex.Message);
            
            var redirectPath = ex.HttpRequestError switch
            {
                System.Net.Http.HttpRequestError.ConnectionError => "/Error/ServiceNotAvailable",
                _ when ex.Message.Contains("Service temporarily unavailable") => "/Error/ServiceNotAvailable",
                _ when ex.Message.Contains("Resource not found") => "/Error/NotFound",
                _ => "/Error/ServiceProblem"
            };

            context.Response.Redirect(redirectPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
            context.Response.Redirect("/Error/ServiceProblem");
        }
    }
}
