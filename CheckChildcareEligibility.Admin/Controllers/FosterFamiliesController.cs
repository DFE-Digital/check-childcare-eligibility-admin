using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.Usecases;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Controllers
{
    public class FosterFamiliesController : BaseController
    {
        private readonly IMenuProvider _menuProvider;
        private readonly ISearchFosterFamiliesRecordsUseCase _searchFosterFamiliesRecordsUseCase;
        private readonly ILoadFosterCarerDetailsUseCase _loadFosterCarerDetailsUseCase;
        private readonly IValidateFosterCarerDetailsUseCase _validateFosterCarerDetailsUseCase;
        private readonly ILoadFosterPartnerDetailsUseCase _loadFosterPartnerDetailsUseCase;
        private readonly IValidateFosterPartnerDetailsUseCase _validateFosterPartnerDetailsUseCase;
        private readonly ILoadFosterChildDetailsUseCase _loadFosterChildDetailsUseCase;
        private readonly IValidateFosterChildDetailsUseCase _validateFosterChildDetailsUseCase;
        private readonly ILoadFosterApplicationSubmittedDateUseCase _loadFosterApplicationSubmittedDateUseCase;
        private readonly IValidateFosterApplicationSubmittedDateUseCase _validateFosterApplicationSubmittedDateUseCase;
        private readonly ICreateFosterFamilyUseCase _createFosterFamilyUseCase;

        public FosterFamiliesController(
            IMenuProvider menuProvider,
            ISearchFosterFamiliesRecordsUseCase searchFosterFamiliesRecordsUseCase,
            ILoadFosterCarerDetailsUseCase loadFosterCarerDetailsUseCase,
            IValidateFosterCarerDetailsUseCase validateFosterCarerDetailsUseCase,
            ILoadFosterPartnerDetailsUseCase loadFosterPartnerDetailsUseCase,
            IValidateFosterPartnerDetailsUseCase validateFosterPartnerDetailsUseCase,
            ILoadFosterChildDetailsUseCase loadFosterChildDetailsUseCase,
            IValidateFosterChildDetailsUseCase validateFosterChildDetailsUseCase,
            ILoadFosterApplicationSubmittedDateUseCase loadFosterApplicationSubmittedDateUseCase,
            IValidateFosterApplicationSubmittedDateUseCase validateFosterApplicationSubmittedDateUseCase,
            ICreateFosterFamilyUseCase createFosterFamilyUseCase,
            IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
        {
            _menuProvider = menuProvider;
            _searchFosterFamiliesRecordsUseCase = searchFosterFamiliesRecordsUseCase;
            _loadFosterCarerDetailsUseCase = loadFosterCarerDetailsUseCase;
            _validateFosterCarerDetailsUseCase = validateFosterCarerDetailsUseCase;
            _loadFosterPartnerDetailsUseCase = loadFosterPartnerDetailsUseCase;
            _validateFosterPartnerDetailsUseCase = validateFosterPartnerDetailsUseCase;
            _loadFosterChildDetailsUseCase = loadFosterChildDetailsUseCase;
            _validateFosterChildDetailsUseCase = validateFosterChildDetailsUseCase;
            _loadFosterApplicationSubmittedDateUseCase = loadFosterApplicationSubmittedDateUseCase;
            _validateFosterApplicationSubmittedDateUseCase = validateFosterApplicationSubmittedDateUseCase;
            _createFosterFamilyUseCase = createFosterFamilyUseCase;
        }

        public async Task<IActionResult> SearchFosterFamiliesRecords(int pageNumber = 1)
        {
            var fosterFamiliesSearchRequest = new FosterFamiliesSearchRequest(pageNumber, 10);
            var response = await _searchFosterFamiliesRecordsUseCase.Execute(fosterFamiliesSearchRequest);

            SearchFosterFamiliesRecordsViewModel vm = new SearchFosterFamiliesRecordsViewModel
            {
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalNumberOfRecords = response.TotalNumberOfRecords,
                Data = response.Data
            };
            return View(vm);
        }

        public async Task<IActionResult> Enter_Foster_Carer_Details(bool clearData = false)
        {
            if (clearData)
            {
                HttpContext.Session.Remove("FosterCarerDetails");
                HttpContext.Session.Remove("FosterPartnerDetails");
                HttpContext.Session.Remove("FosterChildDetails");
                HttpContext.Session.Remove("FosterApplicationSubmittedDate");
                TempData.Remove("Errors");
            }

            var (fosterCarerViewModel, validationErrors) = await _loadFosterCarerDetailsUseCase.Execute(
                HttpContext.Session.GetString("FosterCarerDetails"),
                TempData["Errors"]?.ToString()
                );

            if (validationErrors != null)
                foreach (var (key, errorList) in validationErrors)
                    foreach (var error in errorList)
                        ModelState.AddModelError(key, error);
            return View(fosterCarerViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Enter_Foster_Carer_Details(FosterCarerDetailsViewModel request)
        {
            var validationResult = _validateFosterCarerDetailsUseCase.Execute(request, ModelState);

            if (validationResult == null || !validationResult.IsValid)
            {
                HttpContext.Session.SetString("FosterCarerDetails", JsonConvert.SerializeObject(request));
                TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
                return RedirectToAction("Enter_Foster_Carer_Details");
            }

            // Clear data when starting a new application
            HttpContext.Session.Remove("FosterCarerApplication");

            // Set DateOfBirth in request before serializing
            request.CarerDateOfBirth = new DateTime(
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));

            HttpContext.Session.SetString("FosterCarerDetails", JsonConvert.SerializeObject(request));
            var TEST = HttpContext.Session.GetString("FosterCarerDetails");

            if (request.HasPartner == true)
            {
                return RedirectToAction("Enter_Foster_Partner_Details");
            }
            return RedirectToAction("Enter_Foster_Child_Details");
        }

        public async Task<IActionResult> Enter_Foster_Partner_Details(bool clearData = false)
        {
            var (fosterPartnerDetailsViewModel, validationErrors) = await _loadFosterPartnerDetailsUseCase.Execute(
                HttpContext.Session.GetString("FosterPartnerDetails"),
                TempData["Errors"]?.ToString()
            );

            if (validationErrors != null)
                foreach (var (key, errorList) in validationErrors)
                    foreach (var error in errorList)
                        ModelState.AddModelError(key, error);
            return View(fosterPartnerDetailsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Enter_Foster_Partner_Details(FosterPartnerDetailsViewModel request)
        {
            var validationResult = _validateFosterPartnerDetailsUseCase.Execute(request, ModelState);

            if (validationResult == null || !validationResult.IsValid)
            {
                HttpContext.Session.SetString("FosterPartnerDetails", JsonConvert.SerializeObject(request));
                TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
                return RedirectToAction("Enter_Foster_Partner_Details");
            }

            // Clear data when starting a new application
            HttpContext.Session.Remove("FosterPartnerApplication");

            // Set DateOfBirth in request before serializing
            request.PartnerDateOfBirth = new DateTime(
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));

            HttpContext.Session.SetString("FosterPartnerDetails", JsonConvert.SerializeObject(request));
            var TEST = HttpContext.Session.GetString("FosterPartnerDetails");
            return RedirectToAction("Enter_Foster_Child_Details");
        }

        public async Task<IActionResult> Enter_Foster_Child_Details(bool clearData = false)
        {
            var (fosterChildDetailsViewModel, validationErrors) = await _loadFosterChildDetailsUseCase.Execute(
                HttpContext.Session.GetString("FosterChildDetails"),
                TempData["Errors"]?.ToString()
            );

            if (validationErrors != null)
                foreach (var (key, errorList) in validationErrors)
                    foreach (var error in errorList)
                        ModelState.AddModelError(key, error);
            return View(fosterChildDetailsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Enter_Foster_Child_Details(FosterChildDetailsViewModel request)
        {
            var validationResult = _validateFosterChildDetailsUseCase.Execute(request, ModelState);

            if (validationResult == null || !validationResult.IsValid)
            {
                HttpContext.Session.SetString("FosterChildDetails", JsonConvert.SerializeObject(request));
                TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
                return RedirectToAction("Enter_Foster_Child_Details");
            }

            // Set DateOfBirth in request before serializing
            request.ChildDateOfBirth = new DateTime(
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));

            HttpContext.Session.SetString("FosterChildDetails", JsonConvert.SerializeObject(request));

            return RedirectToAction("Enter_Foster_Application_Submitted_Date_Form");
        }

        public async Task<IActionResult> Enter_Foster_Application_Submitted_Date_Form(bool clearData = false)
        {
            var (fosterApplicationSubmittedDateViewModel, validationErrors) = await _loadFosterApplicationSubmittedDateUseCase.Execute(
                HttpContext.Session.GetString("FosterApplicationSubmittedDate"),
                TempData["Errors"]?.ToString()
            );

            if (validationErrors != null)
                foreach (var (key, errorList) in validationErrors)
                    foreach (var error in errorList)
                        ModelState.AddModelError(key, error);
            return View(fosterApplicationSubmittedDateViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Enter_Foster_Application_Submitted_Date_Form(FosterApplicationSubmittedDateViewModel request)
        {
            var validationResult = _validateFosterApplicationSubmittedDateUseCase.Execute(request, ModelState);

            if (validationResult == null || !validationResult.IsValid)
            {
                HttpContext.Session.SetString("FosterApplicationSubmittedDate", JsonConvert.SerializeObject(request));
                TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
                return RedirectToAction("Enter_Foster_Application_Submitted_Date_Form");
            }

            if (request.IsTodaySelected == true)
            {
                request.SubmissionDate = DateTime.Now;
            }
            else
            {
                // Set DateOfBirth in request before serializing
                request.SubmissionDate = new DateTime(
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));
            }

            HttpContext.Session.SetString("FosterApplicationSubmittedDate", JsonConvert.SerializeObject(request));

            return RedirectToAction("Foster_Application_Check_Details");
        }

        public async Task<IActionResult> Foster_Application_Check_Details()
        {
            var FosterCarerDetails = JsonConvert.DeserializeObject<FosterCarerDetailsViewModel>(HttpContext.Session.GetString("FosterCarerDetails"));
            var FosterPartnerDetails = JsonConvert.DeserializeObject<FosterPartnerDetailsViewModel>(HttpContext.Session.GetString("FosterPartnerDetails"));
            var FosterChildDetails = JsonConvert.DeserializeObject<FosterChildDetailsViewModel>(HttpContext.Session.GetString("FosterChildDetails"));
            var FosterApplicationSubmittedDate = JsonConvert.DeserializeObject<FosterApplicationSubmittedDateViewModel>(HttpContext.Session.GetString("FosterApplicationSubmittedDate"));

            FosterApplicationCheckDetailsViewModel FosterCarerApplication = new FosterApplicationCheckDetailsViewModel();
            FosterCarerApplication.fosterCarerDetailsViewModel = FosterCarerDetails;
            FosterCarerApplication.fosterPartnerDetailsViewModel = FosterPartnerDetails;
            FosterCarerApplication.fosterChildDetailsViewModel = FosterChildDetails;
            FosterCarerApplication.fosterApplicationSubmittedDateViewModel = FosterApplicationSubmittedDate;

            return View("Foster_Application_Check_Details", FosterCarerApplication);
        }

        [HttpPost]
        public async Task<IActionResult> Foster_Application_Check_Details(FosterApplicationCheckDetailsViewModel request)
        {
            var fosterFamilyRequest = new FosterFamilyRequest();
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var fosterCarerRequest = new FosterCarerRequest();
            foreach (var item in request.fosterCarerDetailsViewModel.GetType().GetProperties())
            {
                var value = item.GetValue(request.fosterCarerDetailsViewModel);
                fosterCarerRequest.GetType().GetProperty(item.Name)?.SetValue(fosterCarerRequest, value);
            }
            fosterCarerRequest.LocalAuthorityID = laID;

            var fosterPartnerRequest = new FosterPartnerRequest();
            foreach (var item in request.fosterPartnerDetailsViewModel.GetType().GetProperties())
            {
                var value = item.GetValue(request.fosterPartnerDetailsViewModel);
                fosterPartnerRequest.GetType().GetProperty(item.Name)?.SetValue(fosterPartnerRequest, value);
            }

            var fosterChildRequest = new FosterChildRequest();
            foreach (var item in request.fosterChildDetailsViewModel.GetType().GetProperties())
            {
                var value = item.GetValue(request.fosterChildDetailsViewModel);
                fosterChildRequest.GetType().GetProperty(item.Name)?.SetValue(fosterChildRequest, value);
            }

            fosterFamilyRequest.FosterCarer = fosterCarerRequest;
            fosterFamilyRequest.HasPartner = fosterCarerRequest.HasPartner == true;
            fosterFamilyRequest.Partner = fosterPartnerRequest;
            fosterFamilyRequest.FosterChild = fosterChildRequest;
            fosterFamilyRequest.SubmissionDate = request.fosterApplicationSubmittedDateViewModel.SubmissionDate;

            try
            {
                var response = await _createFosterFamilyUseCase.Execute(fosterFamilyRequest, laID);
                FosterFamilyCreatedViewModel vm = new FosterFamilyCreatedViewModel
                {
                    FosterCarerId = response.FosterCarerId,
                    ChildName = response.ChildName,
                    EligibilityCode = response.EligibilityCode,
                    Status = response.Status,
                    EligibilityConfirmed = response.EligibilityConfirmed,
                    ReconfirmBetween = response.ReconfirmBetween,
                    GracePeriodEndDate = response.GracePeriodEndDate
                };
                return View("Foster_Family_Code_Created", vm);
            }
            catch (BadHttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Foster_Application_Check_Details");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
