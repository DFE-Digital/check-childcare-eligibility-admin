using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeLabels;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Controllers;

public class WorkingFamiliesCheckController : BaseController
{
    private readonly IGetCheckStatusUseCase _getCheckStatusUseCase;
    private readonly ILoadParentDetailsUseCase _loadParentDetailsUseCase;
    private readonly ILoadParentAndChildDetailsUseCase _loadParentAndChildDetailsUseCase;
    private readonly ILogger<WorkingFamiliesCheckController> _logger;
    private readonly IPerformWFEligibilityCheckUseCase _performWFEligibilityCheckUseCase;
    private readonly IValidateParentAndChildDetailsUseCase _validateParentAndChildDetailsUseCase;


    public WorkingFamiliesCheckController(
        ILogger<WorkingFamiliesCheckController> logger,
        ILoadParentDetailsUseCase loadParentDetailsUseCase,
        ILoadParentAndChildDetailsUseCase loadParentAndChildDetailsUseCase,
        IPerformWFEligibilityCheckUseCase performWFEligibilityCheckUseCase,
        IGetCheckStatusUseCase getCheckStatusUseCase,
        IValidateParentAndChildDetailsUseCase validateParentAndChildDetailsUseCase)
    {

        _logger = logger;
        _loadParentDetailsUseCase = loadParentDetailsUseCase;
        _loadParentAndChildDetailsUseCase = loadParentAndChildDetailsUseCase;
        _performWFEligibilityCheckUseCase = performWFEligibilityCheckUseCase;
        _getCheckStatusUseCase = getCheckStatusUseCase;
        _validateParentAndChildDetailsUseCase = validateParentAndChildDetailsUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Enter_Details_WF(bool clearData = false)
    {
        if (clearData)
        {
            TempData.Remove("ParentAndChildDetails");
            TempData.Remove("Errors");
        }

        string eligibilityType = TempData["eligibilityType"] as string;
        if (eligibilityType == null) { eligibilityType = "Unknown eligibility type"; }
        TempData["eligibilityType"] = eligibilityType;
        string label = EligibilityTypeLabels.Labels.ContainsKey(eligibilityType) ? EligibilityTypeLabels.Labels[eligibilityType] : "Unknown eligibility type";
        TempData["eligibilityTypeLabel"] = label;

        var (parentAndChild, validationErrors) = await _loadParentAndChildDetailsUseCase.Execute(
            TempData["ParentAndChildDetails"]?.ToString(),
            TempData["Errors"]?.ToString()
        );

        if (validationErrors != null)
            foreach (var (key, errorList) in validationErrors)
                foreach (var error in errorList)
                    ModelState.AddModelError(key, error);

        return View(parentAndChild);
    }

    [HttpPost]
    public async Task<IActionResult> Enter_Details_WF(ParentAndChildViewModel request)
    {
        var CombinedValidationResult = _validateParentAndChildDetailsUseCase.Execute(request, ModelState);
        request.Child.ChildDateOfBirth = GetDateOfBirth(request.Child.Day, request.Child.Month, request.Child.Year).ToString();
        if (CombinedValidationResult == null || !CombinedValidationResult.IsValid)
        {
            TempData["ParentAndChildDetails"] = JsonConvert.SerializeObject(request);
            TempData["Errors"] = CombinedValidationResult != null ? JsonConvert.SerializeObject(CombinedValidationResult.Errors) : null;
            return RedirectToAction("Enter_Details_WF");
        }

        TempData["ParentAndChildDetails"] = JsonConvert.SerializeObject(request);

        var eligibilityType = TempData.Peek("EligibilityType")?.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(eligibilityType))
            return View("Outcome/Technical_Error_WF");

        if (string.Equals(eligibilityType, "WF", StringComparison.OrdinalIgnoreCase))
        {
            var response = await _performWFEligibilityCheckUseCase.Execute(request, HttpContext.Session);
            TempData["Response"] = JsonConvert.SerializeObject(response);
        }

        return RedirectToAction("Loader_WF");
    }

    public async Task<IActionResult> Loader_WF()
    {
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

        var responseJson = TempData["Response"] as string;

        try
        {
            var outcome = await _getCheckStatusUseCase.Execute(responseJson, HttpContext.Session);
            CheckEligibilityStatus outcomeStatus = (CheckEligibilityStatus)Enum.Parse(typeof(CheckEligibilityStatus), outcome);

            if (outcome == "queuedForProcessing")
            {
                TempData["Response"] = responseJson;
            }

            _logger.LogError(outcome);

            var parentAndChildDetailsJson = TempData["ParentAndChildDetails"]?.ToString();
            TempData["ParentAndChildDetails"] = parentAndChildDetailsJson;

            var (parentAndChild, validationErrors) = await _loadParentAndChildDetailsUseCase.Execute(
            parentAndChildDetailsJson,
            TempData["Errors"]?.ToString());

            switch (outcomeStatus)
            {
                case CheckEligibilityStatus.queuedForProcessing:

                    return View("Loader_WF", parentAndChild);
                case CheckEligibilityStatus.notFound:
                    return View("Outcome/Not_Found_WF", parentAndChild);
                default:
                    var responseItem = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);
                    var result = await _performWFEligibilityCheckUseCase.GetItemAsync(responseItem.Links.Get_EligibilityCheck);
                    WorkingFamiliesResponseViewModel viewModel = new WorkingFamiliesResponseViewModel()
                    {
                        Response = result.Data
                    };
                    return View("Outcome/Response_WF", viewModel);
            }
        }
        catch (Exception ex)
        {
            return View("Outcome/Technical_Error_WF");
        }
    }

    private DateTime GetDateOfBirth(string? dayStr, string? monthStr, string? yearStr)
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

    public IActionResult WF_Guidance()
    {
        return View("WF_Guidance");
    }

}