using System.Globalization;
using System.Text;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeLabels;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.Usecases;
using CheckChildcareEligibility.Admin.ViewModels;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Controllers;

public class BulkCheckController : BaseController
{
    private const int TotalErrorsToDisplay = 20;
    private readonly ICheckGateway _checkGateway;
    private readonly IConfiguration _config;

    private readonly IParseBulkCheckFileUseCase _parseBulkCheckFileUseCase;

    private readonly ILogger<BulkCheckController> _logger;
    private ILogger<BulkCheckController> _loggerMock;

    public BulkCheckController(
        ILogger<BulkCheckController> logger, 
        ICheckGateway checkGateway,
        IConfiguration configuration,
        IParseBulkCheckFileUseCase parseBulkCheckFileUseCase)
    {
        _config = configuration;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
        _parseBulkCheckFileUseCase = parseBulkCheckFileUseCase;
    }

    public IActionResult Bulk_Check()
    {
        var eligibilityType = TempData["eligibilityType"]?.ToString();
        TempData["eligibilityType"] = eligibilityType;
        var label = EligibilityTypeLabels.Labels.ContainsKey(eligibilityType) ? EligibilityTypeLabels.Labels[eligibilityType] : "Unknown eligibility type";
        TempData["eligibilityTypeLabel"] = label; 
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Bulk_Check(IFormFile fileUpload, string eligibilityType)
    {
        TempData["eligibilityType"] = eligibilityType;

        var timeNow = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("FirstSubmissionTimeStamp")))
        {
            var firstSubmissionTimeStampString = HttpContext.Session.GetString("FirstSubmissionTimeStamp");
            DateTime.TryParse(firstSubmissionTimeStampString, out var firstSubmissionTimeStamp);
            var timein1Hour = firstSubmissionTimeStamp.AddHours(1);

            if (timeNow >= timein1Hour) HttpContext.Session.Remove("BulkSubmissions");
        }

        TempData["Response"] = "data_issue";
        
        var requestItems = new List<CheckEligibilityRequestData>();
        
        if (fileUpload == null || fileUpload.ContentType.ToLower() != "text/csv")
        {
            TempData["ErrorMessage"] = "Select a CSV File";
            return RedirectToAction("Bulk_Check");
        }

        // limit csv submission attempts
        var sessionCount = 0;
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("BulkSubmissions")))
        {
            // set session value as 0 if it didnt exist
            HttpContext.Session.SetInt32("BulkSubmissions", 0);
            // Set time in its own session value
            HttpContext.Session.SetString("FirstSubmissionTimeStamp", DateTime.UtcNow.ToString());
        }
        else
        {
            // if it exists, get the value
            sessionCount = (int)HttpContext.Session.GetInt32("BulkSubmissions");
        }

        // increment
        sessionCount++;
        HttpContext.Session.SetInt32("BulkSubmissions", sessionCount);

        // validate
        if (sessionCount > int.Parse(_config["BulkUploadAttemptLimit"]))
        {
            TempData["ErrorMessage"] =
                $"No more than {_config["BulkUploadAttemptLimit"]} batch check requests can be made per hour";
            return RedirectToAction("Bulk_Check");
        }

        // check not more than 10, if it is return Bulk_Check() with ErrorMessage == too many requests made, wait a bit longer

        var errorsViewModel = new BulkCheckErrorsViewModel();

        try
        {
            var checkRowLimit = int.Parse(_config["BulkEligibilityCheckLimit"]);

            using (var fileStream = fileUpload.OpenReadStream())
            {
                var parsedItems = await _parseBulkCheckFileUseCase.Execute(fileStream, eligibilityType == "EYPP" ? Domain.Enums.CheckEligibilityType.EarlyYearPupilPremium : Domain.Enums.CheckEligibilityType.FreeSchoolMeals);

                if (parsedItems.ValidRequests == null || !parsedItems.ValidRequests.Any())
                {
                    if (!parsedItems.Errors.Any() && string.IsNullOrWhiteSpace(parsedItems.ErrorMessage))
                    {
                        TempData["ErrorMessage"] = "Invalid file content.";
                        errorsViewModel.Errors = new List<CheckRowError>();

                        return RedirectToAction("Bulk_Check");
                    } 
                }

                if (parsedItems.ValidRequests.Count > checkRowLimit)
                {
                    TempData["ErrorMessage"] = $"CSV File cannot contain more than {checkRowLimit} records";
                    return RedirectToAction("Bulk_Check");
                }

                if (!string.IsNullOrWhiteSpace(parsedItems.ErrorMessage))
                {
                    TempData["ErrorMessage"] = parsedItems.ErrorMessage;
                    return RedirectToAction("Bulk_Check");
                }

                if (parsedItems.Errors.Any())
                {
                    errorsViewModel.TotalErrorCount = parsedItems.Errors.Count();
                    
                    var csvRowErrors = parsedItems.Errors.Take(TotalErrorsToDisplay);

                    errorsViewModel.Errors = parsedItems.Errors
                        .Select(error =>
                            new CheckRowError() { LineNumber = error.LineNumber, Message = error.Message });

                    return View("BulkOutcome/Error_Data_Issue", errorsViewModel);
                }

                requestItems.AddRange(parsedItems.ValidRequests);
            }

        }
        catch (Exception ex)
        {
            _logger.LogError("ImportEstablishmentData", ex);

            errorsViewModel.ErrorMessage = ex.Message;
            return View("BulkOutcome/Error_Data_Issue", errorsViewModel);
        }

        var result = await _checkGateway.PostBulkCheck(new CheckEligibilityRequestBulk { Data = requestItems });
        HttpContext.Session.SetString("Get_Progress_Check", result.Links.Get_Progress_Check);
        HttpContext.Session.SetString("Get_BulkCheck_Results", result.Links.Get_BulkCheck_Results);
        return RedirectToAction("Bulk_Loader");
    }
    
    public async Task<IActionResult> Bulk_Loader()
    {
        var result = await _checkGateway.GetBulkCheckProgress(HttpContext.Session.GetString("Get_Progress_Check"));
        if (result != null)
        {
            TempData["totalCounter"] = result.Data.Total;
            TempData["currentCounter"] = result.Data.Complete;
            if (result.Data.Complete >= result.Data.Total) return RedirectToAction("Bulk_check_success");
        }

        return View();
    }

    public async Task<IActionResult> Bulk_check_success()
    {
        return View("BulkOutcome/Success");
    }

    public async Task<IActionResult> Bulk_check_download()
    {
        var resultData =
            await _checkGateway.GetBulkCheckResults(HttpContext.Session.GetString("Get_BulkCheck_Results"));
        var exportData = resultData.Data.Select(x => new BulkFSMExport
        {
            LastName = x.LastName,
            DOB = x.DateOfBirth,
            NI = x.NationalInsuranceNumber,
            // NASS field removed as it's not being used in the application
            Outcome = x.Status.GetFsmStatusDescription()
        });

        var fileName = $"free-school-meal-outcomes-{DateTime.Now.ToString("yyyyMMdd")}.csv";

        var result = WriteCsvToMemory(exportData);
        var memoryStream = new MemoryStream(result);
        return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = fileName };
    }

    private byte[] WriteCsvToMemory(IEnumerable<BulkFSMExport> records)
    {
        using (var memoryStream = new MemoryStream())
        using (var streamWriter = new StreamWriter(memoryStream))
        using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
        {
            csvWriter.WriteRecords(records);
            streamWriter.Flush();
            return memoryStream.ToArray();
        }
    }

    public IActionResult Bulk_Check_Status()
    {
        return View();
    }
}