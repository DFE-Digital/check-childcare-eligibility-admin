using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.Services;
using CheckChildcareEligibility.Admin.Usecases;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace CheckChildcareEligibility.Admin.Controllers
{
    [Route("[controller]")]
    [FeatureGate(Features.FosterFamilies)]
    public class FosterFamiliesController : BaseController
    {

        private readonly ISessionContextService _sessionContextService;
        private readonly ISearchFosterFamiliesRecordsUseCase _searchFosterFamiliesRecordsUseCase;
        private readonly ILoadFosterCarerDetailsUseCase _loadFosterCarerDetailsUseCase;
        private readonly ILoadFosterChildDetailsUseCase _loadFosterChildDetailsUseCase;
        private readonly ILoadFosterPartnerDetailsUseCase _loadFosterPartnerDetailsUseCase;
        private readonly IValidateFosterCarerDetailsUseCase _validateFosterCarerDetailsUseCase;
        private readonly IValidateFosterPartnerDetailsUseCase _validateFosterPartnerDetailsUseCase;
        private readonly IValidateFosterChildDetailsUseCase _validateFosterChildDetailsUseCase;
        private readonly IValidateFosterApplicationSubmittedDateUseCase _validateFosterApplicationSubmittedDateUseCase;
        private readonly IGetFosterFamilyUseCase _getFosterFamilyUseCase;
        private readonly IGetFosterChildUseCase _getFosterChildUseCase;
        private readonly IUpdateFosterCarerUseCase _updateFosterCarerUseCase;
        private readonly ICreateFosterFamilyUseCase _createFosterFamilyUseCase;
        private readonly IPreviewFosterFamilyCodeUseCase _previewFosterCodeUseCase;
        private readonly IUpdateFosterChildUseCase _updateFosterChildUseCase;

        public FosterFamiliesController(
            ISessionContextService sessionContextService,
            ISearchFosterFamiliesRecordsUseCase searchFosterFamiliesRecordsUseCase,
            ILoadFosterCarerDetailsUseCase loadFosterCarerDetailsUseCase,
            ILoadFosterPartnerDetailsUseCase loadFosterPartnerDetailsUseCase,
            ILoadFosterChildDetailsUseCase loadFosterChildDetailsUseCase,
            IValidateFosterCarerDetailsUseCase validateFosterCarerDetailsUseCase,
            IValidateFosterPartnerDetailsUseCase validateFosterPartnerDetailsUseCase,
            IValidateFosterChildDetailsUseCase validateFosterChildDetailsUseCase,
            IValidateFosterApplicationSubmittedDateUseCase validateFosterApplicationSubmittedDateUseCase,
            ICreateFosterFamilyUseCase createFosterFamilyUseCase,
            IGetFosterFamilyUseCase getFosterFamilyUseCase,
            IGetFosterChildUseCase getFosterChildUseCase,
            IUpdateFosterCarerUseCase updateFosterCarerUseCase,
            IPreviewFosterFamilyCodeUseCase previewFosterCodeUseCase,
            IUpdateFosterChildUseCase updateFosterChildUseCase,
            IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
        {
            _sessionContextService = sessionContextService;
            _searchFosterFamiliesRecordsUseCase = searchFosterFamiliesRecordsUseCase;
            _loadFosterCarerDetailsUseCase = loadFosterCarerDetailsUseCase;
            _loadFosterPartnerDetailsUseCase = loadFosterPartnerDetailsUseCase;
            _loadFosterChildDetailsUseCase = loadFosterChildDetailsUseCase;
            _validateFosterCarerDetailsUseCase = validateFosterCarerDetailsUseCase;
            _validateFosterPartnerDetailsUseCase = validateFosterPartnerDetailsUseCase;
            _validateFosterChildDetailsUseCase = validateFosterChildDetailsUseCase;
            _validateFosterApplicationSubmittedDateUseCase = validateFosterApplicationSubmittedDateUseCase;
            _getFosterFamilyUseCase = getFosterFamilyUseCase;
            _getFosterChildUseCase = getFosterChildUseCase;
            _updateFosterCarerUseCase = updateFosterCarerUseCase;
            _createFosterFamilyUseCase = createFosterFamilyUseCase;
            _previewFosterCodeUseCase = previewFosterCodeUseCase;
            _updateFosterChildUseCase = updateFosterChildUseCase;
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search_Records_FF(int pageNumber = 1)
        {
            var fosterFamiliesSearchRequest = new FosterFamiliesSearchRequest(pageNumber, 10);
            var response = await _searchFosterFamiliesRecordsUseCase.Execute(fosterFamiliesSearchRequest);

            SearchFosterFamiliesRecordsViewModel vm = new()
            {
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalNumberOfRecords = response.TotalNumberOfRecords,
                Data = response.Data
            };

            return View(vm);
        }

        [HttpGet("EnterCarer/{contextId?}")]
        public async Task<IActionResult> Enter_Carer_Details_FF(string? contextId)
        {
            FosterCarerDetailsViewModel viewModel;

            // Generate new contextId if not provided
            if (string.IsNullOrEmpty(contextId))
            {
                viewModel = new FosterCarerDetailsViewModel()
                {
                    ContextId = Guid.NewGuid().ToString()
                };
            }
            else
            {
                // Pull the FosterCarerDetailsViewModel from session if it exists    
                viewModel = _sessionContextService.GetSessionData<FosterCarerDetailsViewModel>(contextId, "FosterCarerDetails");
                viewModel ??= new FosterCarerDetailsViewModel { ContextId = contextId };
            }
            return View(viewModel);
        }

        [HttpPost("EnterCarer")]
        public async Task<IActionResult> Enter_Carer_Details_FF(FosterCarerDetailsViewModel request)
        {
            var validationResult = _validateFosterCarerDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return View(request);
            }

            // Populate session context with the FosterCarerDetailsViewModel
            _sessionContextService.SetSessionData(request.ContextId, "FosterCarerDetails", request);

            if (request.HasPartner == true)
            {
                // Redirect to enter partner details if they do not yet exist
                var fosterPartnerDetails = _sessionContextService.GetSessionData<FosterPartnerDetailsViewModel>(request.ContextId, "FosterPartnerDetails");
                if (fosterPartnerDetails == null)
                {
                    return RedirectToAction("Enter_Partner_Details_FF", new { request.ContextId });
                }
                // Otherwise redirect back to check details page as this is a change action
                else
                {
                    return RedirectToAction("Check_Details_FF", new { request.ContextId });
                }
            }
            else
            {
                // Clear partner details
                _sessionContextService.ClearSessionData(request.ContextId, "FosterPartnerDetails");

                // Redirect to enter child details if they do not yet exist
                var fosterChildDetails = _sessionContextService.GetSessionData<FosterChildDetailsViewModel>(request.ContextId, "FosterChildDetails");
                if (fosterChildDetails == null)
                {
                    return RedirectToAction("Enter_Child_Details_FF", new { request.ContextId });
                }
                // Otherwise redirect back to check details page as this is a change action
                else
                {
                    return RedirectToAction("Check_Details_FF", new { request.ContextId });
                }
            }
        }

        [HttpGet("UpdateCarer/{FosterCarerId}")]
        public async Task<IActionResult> Update_Carer_Details_FF(Guid FosterCarerId)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var request = await _getFosterFamilyUseCase.Execute(FosterCarerId, laID);
            var fosterCarerViewModel = await _loadFosterCarerDetailsUseCase.Execute(request);
            return View("Enter_Carer_Details_FF", fosterCarerViewModel);
        }

        [HttpPost("UpdateCarer")]
        public async Task<IActionResult> Update_Carer_Details_FF(FosterCarerDetailsViewModel request)
        {
            var validationResult = _validateFosterCarerDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return View("Enter_Carer_Details_FF", request);
            }

            request.CarerDateOfBirth = new DateTime( // Set DateOfBirth in request before serializing
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));

            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);

            var existingCarer = await _getFosterFamilyUseCase.Execute(request.FosterCarerId, laID, true);
            if (existingCarer == null) { return RedirectToAction("Search_Records_FF"); }

            request.HasPartner = existingCarer.HasPartner;
            UpdateFosterCarerRequest updateRequest = new()
            {
                FosterCarerRequest = request.BuildRequest()
            };

            if (existingCarer.HasPartner)
            {
                updateRequest.FosterPartnerRequest = new FosterPartnerRequest
                {
                    PartnerFirstName = existingCarer.PartnerFirstName,
                    PartnerLastName = existingCarer.PartnerLastName,
                    PartnerDateOfBirth = existingCarer.PartnerDateOfBirth.Value,
                    PartnerNationalInsuranceNumber = existingCarer.PartnerNationalInsuranceNumber
                };
            }

            await _updateFosterCarerUseCase.Execute(request.FosterCarerId, laID, updateRequest);
            return RedirectToAction("Family_Record_FF", new { request.FosterCarerId, Confirmation = "Changes to carer saved" });
        }

        [HttpGet("EnterPartner/{contextId}")]
        public async Task<IActionResult> Enter_Partner_Details_FF(string contextId)
        {
            // Pull the FosterCarerDetailsViewModel from session if it exists
            var fosterCarerDetails = _sessionContextService.GetSessionData<FosterCarerDetailsViewModel>(contextId, "FosterCarerDetails");

            // If fosterCarerDetails is null, redirect to enter carer to restart the journey
            if (fosterCarerDetails == null) { return RedirectToAction("Enter_Carer_Details_FF"); }

            // Pull the FosterPartnerDetailsViewModel from session if it exists, otherwise initialise with contextId
            var fosterPartnerDetails = _sessionContextService.GetSessionData<FosterPartnerDetailsViewModel>(contextId, "FosterPartnerDetails");
            fosterPartnerDetails ??= new FosterPartnerDetailsViewModel { ContextId = contextId };

            return View(fosterPartnerDetails);
        }

        [HttpPost("EnterPartner")]
        public async Task<IActionResult> Enter_Partner_Details_FF(FosterPartnerDetailsViewModel request)
        {
            var validationResult = _validateFosterPartnerDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return View(request);
            }

            // Populate session context with the FosterCarerDetailsViewModel
            _sessionContextService.SetSessionData(request.ContextId, "FosterPartnerDetails", request);

            // Redirect to enter child details if they do not yet exist
            var fosterChildDetails = _sessionContextService.GetSessionData<FosterChildDetailsViewModel>(request.ContextId, "FosterChildDetails");
            if (fosterChildDetails == null)
            {
                return RedirectToAction("Enter_Child_Details_FF", new { request.ContextId });
            }
            // Otherwise redirect back to check details page as this is a change action
            else
            {
                return RedirectToAction("Check_Details_FF", new { request.ContextId });
            }
        }


        [HttpGet("UpdatePartner/{FosterCarerId}")]
        public async Task<IActionResult> Update_Partner_Details_FF(Guid FosterCarerId)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var request = await _getFosterFamilyUseCase.Execute(FosterCarerId, laID);
            var viewModel = await _loadFosterPartnerDetailsUseCase.Execute(request);
            return View("Enter_Partner_Details_FF", viewModel);
        }

        [HttpPost("UpdatePartner")]
        public async Task<IActionResult> Update_Partner_Details_FF(FosterPartnerDetailsViewModel request)
        {
            var validationResult = _validateFosterPartnerDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return View("Enter_Partner_Details_FF", request);
            }

            request.PartnerDateOfBirth = new DateTime( // Set DateOfBirth in request before serializing
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));

            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var response = await _getFosterFamilyUseCase.Execute(request.FosterCarerId, laID, true);

            UpdateFosterCarerRequest updateRequest = new()
            {
                FosterCarerRequest = new FosterCarerRequest
                {
                    CarerFirstName = response.CarerFirstName,
                    CarerLastName = response.CarerLastName,
                    CarerDateOfBirth = response.CarerDateOfBirth,
                    CarerNationalInsuranceNumber = response.CarerNationalInsuranceNumber,
                    HasPartner = true
                },
                FosterPartnerRequest = request.BuildRequest()
            };

            await _updateFosterCarerUseCase.Execute(request.FosterCarerId, laID, updateRequest);
            return RedirectToAction("Family_Record_FF", new
            {
                request.FosterCarerId,
                Confirmation = response.HasPartner ?
                    "Changes to partner saved" :
                    "Partner added"
            });
        }

        [HttpGet("RemovePartner/{FosterCarerId}")]
        public async Task<IActionResult> Remove_Partner_Details_FF(Guid FosterCarerId)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var request = await _getFosterFamilyUseCase.Execute(FosterCarerId, laID);
            var viewModel = await _loadFosterPartnerDetailsUseCase.Execute(request);
            return View("Remove_Partner_Details_FF", viewModel);
        }

        [HttpPost("RemovePartner")]
        public async Task<IActionResult> Remove_Partner_Details_FF(FosterPartnerDetailsViewModel request)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var response = await _getFosterFamilyUseCase.Execute(request.FosterCarerId, laID, true);

            UpdateFosterCarerRequest updateRequest = new()
            {
                FosterCarerRequest = new FosterCarerRequest
                {
                    CarerFirstName = response.CarerFirstName,
                    CarerLastName = response.CarerLastName,
                    CarerDateOfBirth = response.CarerDateOfBirth,
                    CarerNationalInsuranceNumber = response.CarerNationalInsuranceNumber,
                    HasPartner = false
                }
            };
            await _updateFosterCarerUseCase.Execute(request.FosterCarerId, laID, updateRequest);
            return RedirectToAction("Family_Record_FF", new { request.FosterCarerId, Confirmation = "Partner removed" });
        }


        [HttpGet("EnterChild/{contextId}")]
        public async Task<IActionResult> Enter_Child_Details_FF(string contextId)
        {
            // Pull the FosterCarerDetailsViewModel from session if it exists
            var fosterCarerDetails = _sessionContextService.GetSessionData<FosterCarerDetailsViewModel>(contextId, "FosterCarerDetails");

            // If fosterCarerDetails is null, redirect to enter carer to restart the journey
            if (fosterCarerDetails == null) { return RedirectToAction("Enter_Carer_Details_FF"); }

            // Pull the fosterChildDetailsViewModel from session if it exists, otherwise initialise with contextId
            var fosterChildDetails = _sessionContextService.GetSessionData<FosterChildDetailsViewModel>(contextId, "FosterChildDetails");
            fosterChildDetails ??= new FosterChildDetailsViewModel
            {
                ContextId = contextId,
                HasPartner = fosterCarerDetails.HasPartner
            };

            return View(fosterChildDetails);
        }

        [HttpPost("EnterChild")]
        public async Task<IActionResult> Enter_Child_Details_FF(FosterChildDetailsViewModel request)
        {
            var validationResult = _validateFosterChildDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return View(request);
            }

            // Populate session context with the FosterCarerDetailsViewModel
            _sessionContextService.SetSessionData(request.ContextId, "FosterChildDetails", request);

            // Redirect to enter submitted date details if they do not yet exist
            var submittedDateDetails = _sessionContextService.GetSessionData<FosterApplicationSubmittedDateViewModel>(request.ContextId, "FosterChildDetails");
            if (submittedDateDetails == null)
            {
                return RedirectToAction("Enter_Submitted_Date_Details_FF", new { request.ContextId });
            }
            // Otherwise redirect back to check details page as this is a change action
            else
            {
                return RedirectToAction("Check_Details_FF", new { request.ContextId });
            }
        }

        [HttpGet("UpdateChild/{FosterChildId}")]
        public async Task<IActionResult> Update_Child_Details_FF(Guid FosterChildId)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var request = await _getFosterChildUseCase.Execute(FosterChildId, laID);
            var fosterChildViewModel = await _loadFosterChildDetailsUseCase.Execute(request);
            return View("Enter_Child_Details_FF", fosterChildViewModel);
        }


        [HttpPost("UpdateChild")]
        public async Task<IActionResult> Update_Child_Details_FF(FosterChildDetailsViewModel request)
        {
            var validationResult = _validateFosterChildDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return View("Enter_Child_Details_FF", request);
            }

            request.ChildDateOfBirth = new DateTime( // Set DateOfBirth in request before serializing
                int.Parse(request.Year),
                int.Parse(request.Month),
                int.Parse(request.Day));

            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var response = await _getFosterChildUseCase.Execute(request.FosterChildId, laID, true);

            UpdateFosterChildRequest updateRequest = new()
            {
                FosterChildRequest = request.BuildRequest()
            };

            await _updateFosterChildUseCase.Execute(request.FosterChildId, laID, updateRequest);
            return RedirectToAction("Code_Record_FF", new
            {
                request.FosterChildId,
                Confirmation = "Changes to child saved"
            });
        }

        [HttpGet("SubmittedDate/{contextId}")]
        public async Task<IActionResult> Enter_Submitted_Date_Details_FF(string contextId)
        {
            // Pull the FosterCarerDetailsViewModel from session if it exists
            var fosterCarerDetails = _sessionContextService.GetSessionData<FosterCarerDetailsViewModel>(contextId, "FosterCarerDetails");

            // If fosterCarerDetails is null, redirect to enter carer to restart the journey
            if (fosterCarerDetails == null) { return RedirectToAction("Enter_Carer_Details_FF"); }

            // Pull the FosterChildDetailsViewModel from session if it exists
            var fosterChildDetails = _sessionContextService.GetSessionData<FosterChildDetailsViewModel>(contextId, "FosterChildDetails");

            // If fosterChildDetails is null, redirect to enter child to complete required details
            if (fosterChildDetails == null) { return RedirectToAction("Enter_Child_Details_FF", new { contextId }); }

            // Pull the fosterChildDetailsViewModel from session if it exists, otherwise initialise with contextId
            var fosterApplicationSubmittedDateViewModel = _sessionContextService.GetSessionData<FosterApplicationSubmittedDateViewModel>(contextId, "FosterApplicationSubmittedDate");
            fosterApplicationSubmittedDateViewModel ??= new FosterApplicationSubmittedDateViewModel { ContextId = contextId };

            return View(fosterApplicationSubmittedDateViewModel);
        }

        [HttpPost("SubmittedDate")]
        public async Task<IActionResult> Enter_Submitted_Date_Details_FF(FosterApplicationSubmittedDateViewModel request)
        {
            var validationResult = _validateFosterApplicationSubmittedDateUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return View(request);
            }

            // Populate session context with the FosterCarerDetailsViewModel
            _sessionContextService.SetSessionData(request.ContextId, "FosterApplicationSubmittedDate", request);

            return RedirectToAction("Check_Details_FF", new { request.ContextId });
        }

        [HttpGet("CheckDetails/{contextId}")]
        public async Task<IActionResult> Check_Details_FF(string contextId)
        {
            // Pull the FosterCarerDetailsViewModel from session if it exists
            var fosterCarerDetails = _sessionContextService.GetSessionData<FosterCarerDetailsViewModel>(contextId, "FosterCarerDetails");
            // If fosterCarerDetails is null, redirect to enter carer to restart the journey
            if (fosterCarerDetails == null) { return RedirectToAction("Enter_Carer_Details_FF"); }

            FosterPartnerDetailsViewModel fosterPartnerDetails = null;
            if (fosterCarerDetails.HasPartner == true)
            {
                // Pull the FosterPartnerDetailsViewModel from session if it exists
                fosterPartnerDetails = _sessionContextService.GetSessionData<FosterPartnerDetailsViewModel>(contextId, "FosterPartnerDetails");
                // If fosterPartnerDetails is null, redirect to enter partner to complete required details
                if (fosterPartnerDetails == null) { return RedirectToAction("Enter_Partner_Details_FF", new { contextId }); }
            }

            // Pull the FosterChildDetailsViewModel from session if it exists
            var fosterChildDetails = _sessionContextService.GetSessionData<FosterChildDetailsViewModel>(contextId, "FosterChildDetails");
            // If fosterChildDetails is null, redirect to enter child to complete required details
            if (fosterChildDetails == null) { return RedirectToAction("Enter_Child_Details_FF", new { contextId }); }

            // Pull the FosterApplicationSubmittedDateViewModel from session if it exists
            var fosterApplicationSubmittedDate = _sessionContextService.GetSessionData<FosterApplicationSubmittedDateViewModel>(contextId, "FosterApplicationSubmittedDate");
            // If fosterApplicationSubmittedDate is null, redirect to submission date to complete required details
            if (fosterApplicationSubmittedDate == null) { return RedirectToAction("Enter_Submitted_Date_Details_FF", new { contextId }); }


            var fosterCodePreview = await _previewFosterCodeUseCase.Execute(
                new FosterFamilyRequest
                {
                    FosterCarer = fosterCarerDetails.BuildRequest(),
                    FosterChild = fosterChildDetails.BuildRequest(),
                    Partner = fosterPartnerDetails?.BuildRequest(),
                    HasPartner = fosterCarerDetails.HasPartner,
                    SubmissionDate = fosterApplicationSubmittedDate.SubmissionDate
                }, int.Parse(_Claims.Organisation.EstablishmentNumber)
            );

            FosterApplicationCheckDetailsViewModel fosterCarerApplication = new()
            {
                ContextId = contextId,
                FosterCarerDetailsViewModel = fosterCarerDetails,
                FosterPartnerDetailsViewModel = fosterPartnerDetails,
                FosterChildDetailsViewModel = fosterChildDetails,
                FosterApplicationSubmittedDateViewModel = fosterApplicationSubmittedDate,
                FosterCodePreview = fosterCodePreview
            };

            return View(fosterCarerApplication);
        }


        [HttpPost("CheckDetails")]
        public async Task<IActionResult> Check_Details_FF(FosterApplicationCheckDetailsViewModel request)
        {
            // If session context is not valid redirect to enter carer to restart the journey
            if (request == null || string.IsNullOrEmpty(request.ContextId)) { return RedirectToAction("Enter_Carer_Details_FF"); }

            // Pull the FosterCarerDetailsViewModel from session if it exists
            var fosterCarerDetails = _sessionContextService.GetSessionData<FosterCarerDetailsViewModel>(request.ContextId, "FosterCarerDetails");
            // If fosterCarerDetails is null, redirect to enter carer to restart the journey
            if (fosterCarerDetails == null) { return RedirectToAction("Enter_Carer_Details_FF"); }

            FosterPartnerDetailsViewModel fosterPartnerDetails = null;
            if (fosterCarerDetails.HasPartner == true)
            {
                // Pull the FosterPartnerDetailsViewModel from session if it exists
                fosterPartnerDetails = _sessionContextService.GetSessionData<FosterPartnerDetailsViewModel>(request.ContextId, "FosterPartnerDetails");
                // If fosterPartnerDetails is null, redirect to enter partner to complete required details
                if (fosterPartnerDetails == null) { return RedirectToAction("Enter_Partner_Details_FF", new { request.ContextId }); }
            }

            // Pull the FosterChildDetailsViewModel from session if it exists
            var fosterChildDetails = _sessionContextService.GetSessionData<FosterChildDetailsViewModel>(request.ContextId, "FosterChildDetails");
            // If fosterChildDetails is null, redirect to enter child to complete required details
            if (fosterChildDetails == null) { return RedirectToAction("Enter_Child_Details_FF", new { request.ContextId }); }

            // Pull the FosterApplicationSubmittedDateViewModel from session if it exists
            var fosterApplicationSubmittedDate = _sessionContextService.GetSessionData<FosterApplicationSubmittedDateViewModel>(request.ContextId, "FosterApplicationSubmittedDate");
            // If fosterApplicationSubmittedDate is null, redirect to submission date to complete required details
            if (fosterApplicationSubmittedDate == null) { return RedirectToAction("Enter_Submitted_Date_Details_FF", new { request.ContextId }); }

            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);

            // Prepare request
            var fosterFamilyRequest = new FosterFamilyRequest
            {
                FosterCarer = fosterCarerDetails.BuildRequest(),
                FosterChild = fosterChildDetails.BuildRequest(),
                Partner = fosterPartnerDetails?.BuildRequest(),
                HasPartner = fosterCarerDetails.HasPartner,
                SubmissionDate = fosterApplicationSubmittedDate.SubmissionDate,
            };

            fosterFamilyRequest.FosterCarer.LocalAuthorityID = laID;
            try
            {
                var response = await _createFosterFamilyUseCase.Execute(fosterFamilyRequest, laID);
                return RedirectToAction("Code_Created_FF", new { response.FosterChildId });
            }
            catch (BadHttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Check_Details_FF", new { request.ContextId });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return RedirectToAction("Check_Details_FF", new { request.ContextId });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("CodeCreated/{FosterChildId}")]
        public async Task<IActionResult> Code_Created_FF(Guid FosterChildId)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var response = await _getFosterChildUseCase.Execute(FosterChildId, laID, true);
            var viewModel = new FosterFamilyCreatedViewModel { Response = response };
            return View(viewModel);
        }

        [HttpGet("Family/{FosterCarerId}")]
        public async Task<IActionResult> Family_Record_FF(Guid FosterCarerId,
            string? confirmation
        )
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var response = await _getFosterFamilyUseCase.Execute(FosterCarerId, laID, true);
            var viewModel = new FosterFamilyViewModel()
            {
                Response = response,
                Confirmation = confirmation
            };
            return View(viewModel);
        }

        [HttpGet("Code/{FosterChildId}")]
        public async Task<IActionResult> Code_Record_FF(Guid FosterChildId)
        {
            var laID = int.Parse(_Claims.Organisation.EstablishmentNumber);
            var childResponse = await _getFosterChildUseCase.Execute(FosterChildId, laID, true);
            var viewModel = new FosterFamiliesCodeResponseViewModel()
            {
                Response = childResponse,
                CodeProperties = new EligibilityCodeProperties(childResponse)
            };
            return View(viewModel);
        }
    }
}
