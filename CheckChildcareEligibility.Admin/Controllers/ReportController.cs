using CheckChildcareEligibility.Admin.Controllers.Constants;
using CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeConstants;
using CheckChildcareEligibility.Admin.Gateways;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;

namespace CheckChildcareEligibility.Admin.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IMenuProvider _menuProvider;
        private readonly IPerformEligibilityCodeHistoryReportUseCase _performEligibilityCodeHistoryReportUseCase;
        private readonly IValidateEligibilityCodeUseCase _validateEligibilityCodeUseCase;
        public ReportController(
             IMenuProvider menuProvider,
             IPerformEligibilityCodeHistoryReportUseCase performEligibilityCodeHistoryReportUse,
             IValidateEligibilityCodeUseCase validateEligibilityCodeUseCase,
            IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
        {
            _menuProvider = menuProvider;
            _performEligibilityCodeHistoryReportUseCase = performEligibilityCodeHistoryReportUse;
            _validateEligibilityCodeUseCase = validateEligibilityCodeUseCase;
        }
        public IActionResult Reports()
        {
            return View();
        }
        public IActionResult Code_Search()
        {
            var model = new EligibilityCodeSearchViewModel();

            var errorsJson = TempData["Errors"]?.ToString();

            if (!string.IsNullOrEmpty(errorsJson))
            {
                var errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(errorsJson);

                foreach (var (key, errorList) in errors)
                {
                    foreach (var error in errorList)
                    {
                        ModelState.AddModelError(key, error);
                    }
                }
            }

            model.EligibilityCode = TempData["EligibilityCode"]?.ToString();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Code_Search(string EligibilityCode)
        {
            var validationResult = _validateEligibilityCodeUseCase.Execute(eligibilityCode);
            if (!validationResult.IsValid)
            {
                TempData["EligibilityCode"] = eligibilityCode;
                TempData["Errors"] =
                JsonConvert.SerializeObject(validationResult.Errors);

                return RedirectToAction("Code_Search");
            }
            var response = await _performEligibilityCodeHistoryReportUseCase.Execute(EligibilityCode);
            var viewModel = new EligibilityCodeHistoryReportViewModel
            {
                EligibilityCode = EligibilityCode,
                Response = response
            };
            return View("Event_History", viewModel);
        }
    }
}
