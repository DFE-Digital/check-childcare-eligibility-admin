using Azure.Core;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Controllers;

public class CheckController : BaseController
{
    private readonly ICheckGateway _checkGateway;
    private readonly IConfiguration _config;
    private readonly IGetCheckStatusUseCase _getCheckStatusUseCase;
    private readonly ILoadParentDetailsUseCase _loadParentDetailsUseCase;
    private readonly ILogger<CheckController> _logger;
    private readonly IPerformEligibilityCheckUseCase _performEligibilityCheckUseCase;
    private readonly IValidateParentDetailsUseCase _validateParentDetailsUseCase;
    

    public CheckController(
        ILogger<CheckController> logger,
        ICheckGateway checkGateway,
        IConfiguration configuration,
        ILoadParentDetailsUseCase loadParentDetailsUseCase,
        IPerformEligibilityCheckUseCase performEligibilityCheckUseCase,
        IGetCheckStatusUseCase getCheckStatusUseCase,
        IValidateParentDetailsUseCase validateParentDetailsUseCase)
    {
        _config = configuration;
        _logger = logger;
        _checkGateway = checkGateway;
        _loadParentDetailsUseCase = loadParentDetailsUseCase;
        _performEligibilityCheckUseCase = performEligibilityCheckUseCase;
        _getCheckStatusUseCase = getCheckStatusUseCase;
        _validateParentDetailsUseCase = validateParentDetailsUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Consent_Declaration()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Consent_Declaration_Approval(string consent)
    {
        if (consent == "checked") return RedirectToAction("Enter_Details");

        return View("Consent_Declaration", true);
    }

    [HttpGet]
    public async Task<IActionResult> Enter_Details()
    {
        var (parent, validationErrors) = await _loadParentDetailsUseCase.Execute(
            TempData["ParentDetails"]?.ToString(),
            TempData["Errors"]?.ToString()
        );

        if (validationErrors != null)
            foreach (var (key, errorList) in validationErrors)
                foreach (var error in errorList)
                    ModelState.AddModelError(key, error);

        return View(parent);
    }

    [HttpPost]
    public async Task<IActionResult> Enter_Details(ParentGuardian request, CheckEligibilityType eligibilityType)
    {
        var validationResult = _validateParentDetailsUseCase.Execute(request, ModelState);

        if (!validationResult.IsValid)
        {
            TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
            TempData["Errors"] = JsonConvert.SerializeObject(validationResult.Errors);
            return RedirectToAction("Enter_Details");
        }

        // Clear data when starting a new application
        TempData.Remove("FsmApplication");
        TempData.Remove("FsmEvidence");

        var response = await _performEligibilityCheckUseCase.Execute(request, HttpContext.Session, eligibilityType);
        TempData["Response"] = JsonConvert.SerializeObject(response);

        return RedirectToAction("Loader");
    }

    public async Task<IActionResult> Loader()
    {
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

        var responseJson = TempData["Response"] as string;
        try
        {
            var outcome = await _getCheckStatusUseCase.Execute(responseJson, HttpContext.Session);

            if (outcome == "queuedForProcessing")
                // Save the response back to TempData for the next poll
                TempData["Response"] = responseJson;

            _logger.LogError(outcome);

            var isLA = _Claims?.Organisation?.Category?.Name == Constants.CategoryTypeLA; //false=school
            switch (outcome)
            {
                case "eligible":
                    return View(isLA ? "Outcome/Eligible_LA" : "Outcome/Eligible");
                    break;

                case "notEligible":
                    return View(isLA ? "Outcome/Not_Eligible_LA" : "Outcome/Not_Eligible");
                    break;

                case "parentNotFound":
                    return View("Outcome/Not_Found");
                    break;

                case "queuedForProcessing":
                    return View("Loader");
                    break;

                default:
                    return View("Outcome/Technical_Error");
            }
        }
        catch (Exception ex)
        {
            return View("Outcome/Technical_Error");
        }
    }


}