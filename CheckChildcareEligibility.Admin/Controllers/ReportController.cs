using CheckChildcareEligibility.Admin.Controllers.Constants;
using CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeConstants;
using CheckChildcareEligibility.Admin.Gateways;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IMenuProvider _menuProvider;
        private readonly IPerformEligibilityCodeHistoryReportUseCase _performEligibilityCodeHistoryReportUseCase;
        public ReportController(
             IMenuProvider menuProvider,
             IPerformEligibilityCodeHistoryReportUseCase performEligibilityCodeHistoryReportUse,
            IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
        {
            _menuProvider = menuProvider;
            _performEligibilityCodeHistoryReportUseCase = performEligibilityCodeHistoryReportUse;
        }
        public IActionResult Reports()
        {
            return View();
        }
        public IActionResult Code_Search()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Code_Search(string EligibilityCode)
        {
            if (EligibilityCode == null)
            {
                TempData["ErrorMessage"] = "Please enter an Eligibility Code";

                return RedirectToAction("Code_Seach");
            }
            var response = await _performEligibilityCodeHistoryReportUseCase.Execute(EligibilityCode);
            var viewModel = new EligibilityCodeHistoryReportViewModel
            {
                EligibilityCode = EligibilityCode,
                Response = response
            };
            return View("Report/Event_History", viewModel);
        }
    }
}
