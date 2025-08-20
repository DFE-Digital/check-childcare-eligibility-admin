using Azure.Core;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeLabels;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
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
    private readonly IPerform2YoEligibilityCheckUseCase _perform2YoEligibilityCheckUseCase;
    private readonly IPerformEyppEligibilityCheckUseCase _performEyppEligibilityCheckUseCase;
    private readonly IValidateParentDetailsUseCase _validateParentDetailsUseCase;


    public CheckController(
        ILogger<CheckController> logger,
        ICheckGateway checkGateway,
        IConfiguration configuration,
        ILoadParentDetailsUseCase loadParentDetailsUseCase,
        IPerform2YoEligibilityCheckUseCase perform2YoEligibilityCheckUseCase,
        IPerformEyppEligibilityCheckUseCase performEyppEligibilityCheckUseCase,
        IGetCheckStatusUseCase getCheckStatusUseCase,
        IValidateParentDetailsUseCase validateParentDetailsUseCase)
    {
        _config = configuration;
        _logger = logger;
        _checkGateway = checkGateway;
        _loadParentDetailsUseCase = loadParentDetailsUseCase;
        _perform2YoEligibilityCheckUseCase = perform2YoEligibilityCheckUseCase;
        _performEyppEligibilityCheckUseCase = performEyppEligibilityCheckUseCase;
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
    public async Task<IActionResult> Enter_Details(bool clearData = false)
    {
        // If clearData is true, remove the ParentDetails from TempData
        if (clearData)
        {
            TempData.Remove("ParentDetails");
            TempData.Remove("Errors");
        }

        var eligibilityType = TempData["eligibilityType"].ToString();
        TempData["eligibilityType"] = eligibilityType;
        var label = EligibilityTypeLabels.Labels.ContainsKey(eligibilityType) ? EligibilityTypeLabels.Labels[eligibilityType] : "Unknown eligibility type";
        TempData["eligibilityTypeLabel"] = label;

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
    public async Task<IActionResult> Enter_Details(ParentGuardian request)
    {
        var validationResult = _validateParentDetailsUseCase.Execute(request, ModelState);

        // Add null check for validation result
        if (validationResult == null || !validationResult.IsValid)
        {
            TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
            TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
            return RedirectToAction("Enter_Details");
        }

        // Clear data when starting a new application
        TempData.Remove("FsmApplication");
        TempData.Remove("FsmEvidence");

        TempData["ParentDetails"] = JsonConvert.SerializeObject(request);

        var eligibilityType = TempData.Peek("EligibilityType")?.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(eligibilityType))
            return View("Outcome/Technical_Error");

        if (string.Equals(eligibilityType, "2YO", StringComparison.OrdinalIgnoreCase))
        {
            var response = await _perform2YoEligibilityCheckUseCase.Execute(request, HttpContext.Session);
            TempData["Response"] = JsonConvert.SerializeObject(response);
        }

        if (string.Equals(eligibilityType, "EYPP", StringComparison.OrdinalIgnoreCase))
        {
            var response = await _performEyppEligibilityCheckUseCase.Execute(request, HttpContext.Session);
            TempData["Response"] = JsonConvert.SerializeObject(response);
        }

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

            var eligibilityType = TempData.Peek("EligibilityType")?.ToString() ?? string.Empty;

            var parentDetailsJson = TempData["ParentDetails"]?.ToString();

            if (parentDetailsJson != null)
            {
                TempData["ParentDetails"] = parentDetailsJson;
            }

            var (parent, validationErrors) = await _loadParentDetailsUseCase.Execute(
                parentDetailsJson,
                TempData["Errors"]?.ToString()
            );

            var eligbilityOutcomeVm = new EligibilityOutcomeViewModel
            {
                EligibilityType = eligibilityType,
                EligibilityTypeLabel = GetEligibilityTypeLabel(eligibilityType),
                ParentLastName = parent?.LastName ?? string.Empty,
                ParentDateOfBirth = GetDateOfBirth(parent?.Day, parent?.Month, parent?.Year).ToString(),
                ParentNino = parent?.NationalInsuranceNumber ?? string.Empty
            };

            var isLA = _Claims?.Organisation?.Category?.Name == Constants.CategoryTypeLA; //false=school
            switch (outcome)
            {
                case "eligible":
                    return View(isLA ? "Outcome/Eligible_LA" : "Outcome/Eligible", eligbilityOutcomeVm);

                case "notEligible":
                    return View(isLA ? "Outcome/Not_Eligible_LA" : "Outcome/Not_Eligible", eligbilityOutcomeVm);

                case "parentNotFound":
                    return View("Outcome/Not_Found", eligbilityOutcomeVm);

                case "queuedForProcessing":
                    return View("Loader", eligbilityOutcomeVm);

                default:
                    return View("Outcome/Technical_Error", eligbilityOutcomeVm);
            }
        }
        catch (Exception ex)
        {
            // Create a minimal view model for the error case
            var eligbilityOutcomeVm = new EligibilityOutcomeViewModel
            {
                EligibilityType = TempData.Peek("EligibilityType")?.ToString() ?? string.Empty,
                EligibilityTypeLabel = GetEligibilityTypeLabel(TempData.Peek("EligibilityType")?.ToString() ?? string.Empty)
            };

            // Try to get parent details again
            if (TempData["ParentDetails"] != null)
            {
                var (parent, _) = await _loadParentDetailsUseCase.Execute(
                    TempData["ParentDetails"]?.ToString(),
                    null
                );

                if (parent != null)
                {
                    eligbilityOutcomeVm.ParentLastName = parent.LastName ?? string.Empty;
                    eligbilityOutcomeVm.ParentDateOfBirth = GetDateOfBirth(parent.Day, parent.Month, parent.Year).ToString();
                    eligbilityOutcomeVm.ParentNino = parent.NationalInsuranceNumber ?? string.Empty;
                }
            }

            return View("Outcome/Technical_Error", eligbilityOutcomeVm);
        }
    }

    public DateTime GetDateOfBirth(string? dayStr, string? monthStr, string? yearStr)
    {
        if (int.TryParse(dayStr, out int day) &&
            int.TryParse(monthStr, out int month) &&
            int.TryParse(yearStr, out int year))
        {
            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        return DateTime.MinValue;
    }

    private string GetEligibilityTypeLabel(string eligibilityType)
    {
        if (string.IsNullOrEmpty(eligibilityType))
            return "eligibility";

        return EligibilityTypeLabels.Labels.ContainsKey(eligibilityType)
            ? EligibilityTypeLabels.Labels[eligibilityType]
            : "eligibility";
    }

}