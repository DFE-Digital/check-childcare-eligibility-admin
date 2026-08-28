using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Usecases;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Controllers
{
    [Route("[controller]")]
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
        private readonly IGetFosterFamilyUseCase _getFosterFamilyUseCase;
        private readonly IGetFosterChildUseCase _getFosterChildUseCase;
        private readonly IUpdateFosterCarerUseCase _updateFosterCarerUseCase;
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
            IGetFosterFamilyUseCase getFosterFamilyUseCase,
            IGetFosterChildUseCase getFosterChildUseCase,
            IUpdateFosterCarerUseCase updateFosterCarerUseCase,
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
            _getFosterFamilyUseCase = getFosterFamilyUseCase;
            _getFosterChildUseCase = getFosterChildUseCase;
            _updateFosterCarerUseCase = updateFosterCarerUseCase;
            _createFosterFamilyUseCase = createFosterFamilyUseCase;
        }

        private void ClearFFSessionDetails() {
            HttpContext.Session.Remove("FosterCarerDetails");
            HttpContext.Session.Remove("FosterPartnerDetails");
            HttpContext.Session.Remove("FosterChildDetails");
            HttpContext.Session.Remove("FosterApplicationSubmittedDate");
            TempData.Remove("Errors");
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search_Records_FF(int pageNumber = 1)
        {
            ClearFFSessionDetails();
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

        [HttpGet("Carer")]
        public async Task<IActionResult> Enter_Carer_Details_FF(bool clearData = false)
        {
            if (clearData)
            {
                ClearFFSessionDetails();
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

        [HttpPost("Carer")]
        public async Task<IActionResult> Enter_Carer_Details_FF(FosterCarerDetailsViewModel request)
        {
            var validationResult = _validateFosterCarerDetailsUseCase.Execute(request, ModelState);

            if (validationResult == null || !validationResult.IsValid)
            {
                HttpContext.Session.SetString("FosterCarerDetails", JsonConvert.SerializeObject(request));
                TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
                return RedirectToAction("Enter_Carer_Details_FF");
            }

            HttpContext.Session.Remove("FosterCarerApplication");
            request.CarerDateOfBirth = new DateTime( // Set DateOfBirth in request before serializing
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));
            HttpContext.Session.SetString("FosterCarerDetails", JsonConvert.SerializeObject(request));

            if (request.HasPartner == true)
            {
                return RedirectToAction("Enter_Partner_Details_FF");
            }
            return RedirectToAction("Enter_Child_Details_FF");
        }

        [HttpGet("UpdateCarer")]
        public async Task<IActionResult> Update_Carer_Details_FF(Guid FosterCarerId)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var request = await _getFosterFamilyUseCase.Execute(FosterCarerId, laID);

            HttpContext.Session.SetString("FosterCarerDetails", JsonConvert.SerializeObject(request));

            var (fosterCarerViewModel, validationErrors) = await _loadFosterCarerDetailsUseCase.Execute(
                HttpContext.Session.GetString("FosterCarerDetails"),
                TempData["Errors"]?.ToString()
                );

            // Set DateOfBirth in request before serializing
            fosterCarerViewModel.Day = request.CarerDateOfBirth.Day.ToString();
            fosterCarerViewModel.Month = request.CarerDateOfBirth.Month.ToString();
            fosterCarerViewModel.Year = request.CarerDateOfBirth.Year.ToString();
            fosterCarerViewModel.IsUpdate = true;
            fosterCarerViewModel.CarerId = FosterCarerId;

            if (validationErrors != null)
                foreach (var (key, errorList) in validationErrors)
                    foreach (var error in errorList)
                        ModelState.AddModelError(key, error);
            return View("Enter_Carer_Details_FF", fosterCarerViewModel);
        }

        [HttpPost("UpdateCarer")]
        public async Task<IActionResult> Update_Carer_Details_FF(FosterCarerDetailsViewModel request)
        {
            var validationResult = _validateFosterCarerDetailsUseCase.Execute(request, ModelState);

            if (validationResult == null || !validationResult.IsValid)
            {
                HttpContext.Session.SetString("FosterCarerDetails", JsonConvert.SerializeObject(request));
                TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
                return RedirectToAction("Enter_Carer_Details_FF");
            }

            request.CarerDateOfBirth = new DateTime( // Set DateOfBirth in request before serializing
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));
            HttpContext.Session.SetString("FosterCarerDetails", JsonConvert.SerializeObject(request));

            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);

            var response = await _getFosterFamilyUseCase.Execute(request.CarerId, laID, true);

            UpdateFosterCarerRequest updateRequest = new UpdateFosterCarerRequest();
            FosterCarerRequest fosterCarerRequest = new FosterCarerRequest
            {
                CarerFirstName = request.CarerFirstName,
                CarerLastName = request.CarerLastName,
                CarerDateOfBirth = request.CarerDateOfBirth,
                CarerNationalInsuranceNumber = request.CarerNationalInsuranceNumber,
                HasPartner = request.HasPartner
            };
            updateRequest.FosterCarerRequest = fosterCarerRequest;
            if (request.HasPartner == true)
            {
                FosterPartnerRequest fosterPartnerRequest = new FosterPartnerRequest
                {
                    PartnerFirstName = response.PartnerFirstName,
                    PartnerLastName = response.PartnerLastName,
                    PartnerDateOfBirth = response.PartnerDateOfBirth.Value,
                    PartnerNationalInsuranceNumber = response.PartnerNationalInsuranceNumber
                };
                updateRequest.FosterPartnerRequest = fosterPartnerRequest;
            }

            await _updateFosterCarerUseCase.Execute(request.CarerId, laID, updateRequest);
            return RedirectToAction("Family_Record_FF", new { request.CarerId, includeChildren = true });
        }

        [HttpGet("Partner")]
        public async Task<IActionResult> Enter_Partner_Details_FF()
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

        [HttpPost("Partner")]
        public async Task<IActionResult> Enter_Partner_Details_FF(FosterPartnerDetailsViewModel request)
        {
            var validationResult = _validateFosterPartnerDetailsUseCase.Execute(request, ModelState);

            if (validationResult == null || !validationResult.IsValid)
            {
                HttpContext.Session.SetString("FosterPartnerDetails", JsonConvert.SerializeObject(request));
                TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
                return RedirectToAction("Enter_Partner_Details_FF");
            }

            // Clear data when starting a new application
            HttpContext.Session.Remove("FosterPartnerApplication");

            // Set DateOfBirth in request before serializing
            request.PartnerDateOfBirth = new DateTime(
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));

            HttpContext.Session.SetString("FosterPartnerDetails", JsonConvert.SerializeObject(request));
            return RedirectToAction("Enter_Child_Details_FF");
        }

        [HttpGet("Child")]
        public async Task<IActionResult> Enter_Child_Details_FF()
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

        [HttpPost("Child")]
        public async Task<IActionResult> Enter_Child_Details_FF(FosterChildDetailsViewModel request)
        {
            var validationResult = _validateFosterChildDetailsUseCase.Execute(request, ModelState);

            if (validationResult == null || !validationResult.IsValid)
            {
                HttpContext.Session.SetString("FosterChildDetails", JsonConvert.SerializeObject(request));
                TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
                return RedirectToAction("Enter_Child_Details_FF");
            }

            // Set DateOfBirth in request before serializing
            request.ChildDateOfBirth = new DateTime(
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));

            HttpContext.Session.SetString("FosterChildDetails", JsonConvert.SerializeObject(request));

            return RedirectToAction("Enter_Submitted_Date_Details_FF");
        }

        [HttpGet("SubmittedDate")]
        public async Task<IActionResult> Enter_Submitted_Date_Details_FF()
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

        [HttpPost("SubmittedDate")]
        [HttpPost]
        public async Task<IActionResult> Enter_Submitted_Date_Details_FF(FosterApplicationSubmittedDateViewModel request)
        {
            var validationResult = _validateFosterApplicationSubmittedDateUseCase.Execute(request, ModelState);

            if (validationResult == null || !validationResult.IsValid)
            {
                HttpContext.Session.SetString("FosterApplicationSubmittedDate", JsonConvert.SerializeObject(request));
                TempData["Errors"] = validationResult != null ? JsonConvert.SerializeObject(validationResult.Errors) : null;
                return RedirectToAction("Enter_Submitted_Date_Details_FF");
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

            return RedirectToAction("Check_Details_FF");
        }

        [HttpGet("CheckDetails")]
        public async Task<IActionResult> Check_Details_FF()
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

            return View("Check_Details_FF", FosterCarerApplication);
        }


        [HttpPost("CheckDetails")]
        public async Task<IActionResult> Check_Details_FF(FosterApplicationCheckDetailsViewModel request)
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
                return RedirectToAction("CodeCreated", vm);
            }
            catch (BadHttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Check_Details_FF");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("CodeCreated")]
        public async Task<IActionResult> Code_Created_FF(FosterFamilyCreatedViewModel request)
        {
            return View(request);
        }

        [HttpGet("Family/{CarerId}")]
        public async Task<IActionResult> Family_Record_FF(Guid CarerId, bool includeChildren = false)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);

            var response = await _getFosterFamilyUseCase.Execute(CarerId, laID, includeChildren);

            return View(response);
        }

        [HttpGet("Code/{CarerId}")]
        public async Task<IActionResult> Code_Record_FF(Guid CarerId, bool includeFosterCarer = false)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var familyResponse = await _getFosterFamilyUseCase.Execute(CarerId, laID, true);
            var childId = familyResponse.FosterChildren[0].FosterChildId;
            var childResponse = await _getFosterChildUseCase.Execute(childId, laID, includeFosterCarer);

            return View(childResponse);
        }
    }
}
