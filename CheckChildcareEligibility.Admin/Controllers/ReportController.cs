using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Controllers.Constants;
using CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeConstants;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace CheckChildcareEligibility.Admin.Controllers
{
    [FeatureGate("Reports")]
    public class ReportController : BaseController
    {
        private readonly IPerformEligibilityCodeHistoryReportUseCase _performEligibilityCodeHistoryReportUseCase;
        private readonly IValidateEligibilityCodeUseCase _validateEligibilityCodeUseCase;
        public ReportController(
             IPerformEligibilityCodeHistoryReportUseCase performEligibilityCodeHistoryReportUse,
             IValidateEligibilityCodeUseCase validateEligibilityCodeUseCase,
            IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
        {
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
            var validationResult = _validateEligibilityCodeUseCase.Execute(EligibilityCode);
            if (!validationResult.IsValid)
            {
                TempData["EligibilityCode"] = EligibilityCode;
                TempData["Errors"] = JsonConvert.SerializeObject(validationResult.Errors);
                return RedirectToAction("Code_Search");
            }
            var response = await _performEligibilityCodeHistoryReportUseCase.Execute(EligibilityCode);
            if (response.Data.Count == 0)
            {
                var noMatchView = new EligibilityCodeSearchViewModel
                {
                    EligibilityCode = EligibilityCode,
                };
                return View("No_Match", noMatchView);
            }
            var viewModel = new EligibilityCodeHistoryReportViewModel
            {
                EligibilityCode = EligibilityCode,
                Response = response
            };
            return View("Event_History", viewModel);
        }

        public async Task<IActionResult> Report_Download(string EligibilityCode)
        {
            var filePrefix = ReportHistoryType.downloadPrefix;

            var exportData = await _performEligibilityCodeHistoryReportUseCase.Execute(EligibilityCode);

            var result = WriteCsvToMemory(exportData);

            var memoryStream = new MemoryStream(result);

            return File(result, "text/csv", $"{filePrefix}-{EligibilityCode}.csv");
        }
        private byte[] WriteCsvToMemory(WorkingFamilyEventByEligibilityCodeResponse eventHistory)
        {
            var exportData = eventHistory.Data.Select(item =>
                new EligibilityCodeHistoryCsvExport
                {
                    Event = item.EventName,

                    SubmissionDate = item.Record?.SubmissionDate?.ToString("dd/MM/yyyy"),
                    DiscretionaryStartDate = item.Record?.DiscretionaryStartDate?.ToString("dd/MM/yyyy"),
                    ValidityStartDate = item.Record?.ValidityStartDate?.ToString("dd/MM/yyyy"),
                    ValidityEndDate = item.Record?.ValidityEndDate?.ToString("dd/MM/yyyy"),
                    GracePeriodEndDate = item.Record?.GracePeriodEndDate?.ToString("dd/MM/yyyy"),
                    EventId = item.Record?.EventId,
                    ParentNationalInsuranceNumber = item.Record?.ParentNationalInsuranceNumber,
                    ParentFirstName = item.Record?.ParentFirstName,
                    ParentLastName = item.Record?.ParentLastName,
                    ParentDateOfBirth = item.Record?.ParentDateOfBirth?.ToString("dd/MM/yyyy"),

                    PartnerNationalInsuranceNumber = item.Record?.PartnerNationalInsuranceNumber,
                    PartnerFirstName = item.Record?.PartnerFirstName,
                    PartnerLastName = item.Record?.PartnerLastName,
                    PartnerDateOfBirth = item.Record?.PartnerDateOfBirth?.ToString("dd/MM/yyyy"),

                    ChildFirstName = item.Record?.ChildFirstName,
                    ChildLastName = item.Record?.ChildLastName,
                    ChildDateOfBirth = item.Record?.ChildDateOfBirth?.ToString("dd/MM/yyyy"),
                    ChildPostCode = item.Record?.ChildPostCode
                })
                .ToList();

            using var memoryStream = new MemoryStream();

            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(exportData);
            }

            return memoryStream.ToArray();
        }
    }
}
