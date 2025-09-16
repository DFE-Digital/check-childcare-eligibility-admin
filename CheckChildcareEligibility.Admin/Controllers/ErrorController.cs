using System.Diagnostics;
using CheckChildcareEligibility.Admin.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<CheckController> _logger;

    public ErrorController(ILogger<CheckController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        // Get the details of the exception that occurred
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public new IActionResult NotFound()
    {
        return View();
    }

    public IActionResult ServiceProblem()
    {
        return View();
    }

    public IActionResult ServiceNotAvailable()
    {
        return View();
    }

    // Status code routing - these match the pattern /Error/{statusCode}
    public IActionResult Error404()
    {
        return View("NotFound");
    }

    public IActionResult Error500()
    {
        return View("ServiceProblem");
    }

    public IActionResult Error503()
    {
        return View("ServiceNotAvailable");
    }
}