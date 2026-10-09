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
        private readonly IValidateFosterCodeReconfirmDateUseCase _validateFosterCodeReconfirmDateUseCase;
        private readonly IGetFosterFamilyUseCase _getFosterFamilyUseCase;
        private readonly IGetFosterChildUseCase _getFosterChildUseCase;
        private readonly IUpdateFosterCarerUseCase _updateFosterCarerUseCase;
        private readonly ICreateFosterFamilyUseCase _createFosterFamilyUseCase;
        private readonly IPreviewFosterFamilyCodeUseCase _previewFosterCodeUseCase;
        private readonly IPreviewFosterCodeReconfirmUseCase _previewFosterCodeReconfirmUseCase;
        private readonly IReconfirmFosterCodeUseCase _reconfirmFosterCodeUseCase;
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
            IValidateFosterCodeReconfirmDateUseCase validateFosterCodeReconfirmDateUseCase,
            ICreateFosterFamilyUseCase createFosterFamilyUseCase,
            IGetFosterFamilyUseCase getFosterFamilyUseCase,
            IGetFosterChildUseCase getFosterChildUseCase,
            IUpdateFosterCarerUseCase updateFosterCarerUseCase,
            IPreviewFosterFamilyCodeUseCase previewFosterCodeUseCase,
            IPreviewFosterCodeReconfirmUseCase previewFosterCodeReconfirmUseCase,
            IReconfirmFosterCodeUseCase reconfirmFosterCodeUseCase,
            IUpdateFosterChildUseCase updateFosterChildUseCase,
            IDfeSignInApiService dfeSignInApiService,
            IConfiguration configuration) : base(dfeSignInApiService, configuration)
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
            _validateFosterCodeReconfirmDateUseCase = validateFosterCodeReconfirmDateUseCase;
            _getFosterFamilyUseCase = getFosterFamilyUseCase;
            _getFosterChildUseCase = getFosterChildUseCase;
            _updateFosterCarerUseCase = updateFosterCarerUseCase;
            _createFosterFamilyUseCase = createFosterFamilyUseCase;
            _previewFosterCodeUseCase = previewFosterCodeUseCase;
            _previewFosterCodeReconfirmUseCase = previewFosterCodeReconfirmUseCase;
            _reconfirmFosterCodeUseCase = reconfirmFosterCodeUseCase;
            _updateFosterChildUseCase = updateFosterChildUseCase;
        }

        private void DetectJourneyRestartAndClearSession(string sessionId, string sessionKey, string journeyName)
        {
            // Detect referrer pattern - if journeyName is not the final or penultimate segment then 
            // we should clear session data to restart the journey
            string[] referrer = HttpContext.Request.Headers.Referer.ToString()
                .Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (referrer.Length == 0 ||
                (referrer[^1] != journeyName && (referrer.Length < 2 || referrer[^2] != journeyName)))
            {
                // Clear session data to restart the journey
                _sessionContextService.ClearSessionData(sessionId, sessionKey);
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search_Records_FF(int pageNumber = 1, string ninoFilter = "")
        {
            var fosterFamiliesSearchRequest = new FosterFamiliesSearchRequest(pageNumber, 10, ninoFilter);
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

                // Validate the model if it exists to allow post-redirect-get behaviour for validation messages
                if (viewModel != null)
                {
                    TryValidateModel(viewModel);
                    await _validateFosterCarerDetailsUseCase.Execute(viewModel, ModelState);
                }

                viewModel ??= new FosterCarerDetailsViewModel { ContextId = contextId };
            }
            return View(viewModel);
        }

        [HttpPost("EnterCarer")]
        public async Task<IActionResult> Enter_Carer_Details_FF(FosterCarerDetailsViewModel request)
        {
            // Populate session context with the FosterCarerDetailsViewModel before validation
            _sessionContextService.SetSessionData(request.ContextId, "FosterCarerDetails", request);

            var validationResult = await _validateFosterCarerDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                // If validation failure was caused by a duplicate carer conflict, return conflict view
                if (validationResult != null && validationResult.ConflictingFosterCarerId != Guid.Empty)
                {
                    var conflictViewModel = new FosterCarerConflictViewModel()
                    {
                        CarerDetails = request,
                        ConflictingFamily = await _getFosterFamilyUseCase.Execute(validationResult.ConflictingFosterCarerId, false)
                    };
                    return View("Resolve_Carer_Conflict_FF", conflictViewModel);
                }
                // Redirect back to get page to show validation errors
                return RedirectToAction("Enter_Carer_Details_FF", new { request.ContextId });
            }

            // Update session context with the FosterCarerDetailsViewModel post successful validation
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
        public async Task<IActionResult> Update_Carer_Details_FF(Guid fosterCarerId)
        {
            // Load foster carer record to ensure it exists and we have access
            var request = await _getFosterFamilyUseCase.Execute(fosterCarerId);

            // Clear session to restart journey if required
            DetectJourneyRestartAndClearSession(fosterCarerId.ToString(), "FosterCarerDetails", "UpdateCarer");

            // Pull the FosterCarerDetailsViewModel from session if it exists    
            var viewModel = _sessionContextService.GetSessionData<FosterCarerDetailsViewModel>(fosterCarerId.ToString(), "FosterCarerDetails");

            if (viewModel != null)
            {
                // Validate the model if it exists to allow post-redirect-get behaviour for validation messages
                TryValidateModel(viewModel);
                await _validateFosterCarerDetailsUseCase.Execute(viewModel, ModelState);
            }
            else
            {
                // Otherwise load the view model from the use case if it does not exist in session
                viewModel = await _loadFosterCarerDetailsUseCase.Execute(request);
            }

            return View("Enter_Carer_Details_FF", viewModel);
        }

        [HttpPost("UpdateCarer")]
        public async Task<IActionResult> Update_Carer_Details_FF(FosterCarerDetailsViewModel request)
        {
            // Load foster carer record to ensure it exists and we have access
            var existingCarer = await _getFosterFamilyUseCase.Execute(request.FosterCarerId, true);

            // Populate session context with the FosterCarerDetailsViewModel before validation
            _sessionContextService.SetSessionData(request.FosterCarerId.ToString(), "FosterCarerDetails", request);

            var validationResult = await _validateFosterCarerDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                if (validationResult != null && validationResult.ConflictingFosterCarerId != Guid.Empty)
                {
                    var conflictViewModel = new FosterCarerConflictViewModel()
                    {
                        CarerDetails = request,
                        ConflictingFamily = await _getFosterFamilyUseCase.Execute(validationResult.ConflictingFosterCarerId, false)
                    };
                    return View("Resolve_Carer_Conflict_FF", conflictViewModel);
                }
                return RedirectToAction("Update_Carer_Details_FF", new { request.FosterCarerId });
            }

            // Build update request
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
            await _updateFosterCarerUseCase.Execute(request.FosterCarerId, updateRequest);
            
            // Clear carer details from session
            _sessionContextService.ClearSessionData(request.FosterCarerId.ToString(), "FosterCarerDetails");

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
            var viewModel = _sessionContextService.GetSessionData<FosterPartnerDetailsViewModel>(contextId, "FosterPartnerDetails");

            // Validate the model if it exists to allow post-redirect-get behaviour for validation messages
            if (viewModel != null)
            {
                TryValidateModel(viewModel);
                await _validateFosterPartnerDetailsUseCase.Execute(viewModel, ModelState);
            }

            viewModel ??= new FosterPartnerDetailsViewModel { ContextId = contextId };

            return View(viewModel);
        }

        [HttpPost("EnterPartner")]
        public async Task<IActionResult> Enter_Partner_Details_FF(FosterPartnerDetailsViewModel request)
        {
            // Populate session context with the FosterCarerDetailsViewModel before validation
            _sessionContextService.SetSessionData(request.ContextId, "FosterPartnerDetails", request);

            var validationResult = await _validateFosterPartnerDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                if (validationResult != null && validationResult.ConflictingFosterCarerId != Guid.Empty)
                {
                    var conflictViewModel = new FosterPartnerConflictViewModel()
                    {
                        PartnerDetails = request,
                        ConflictingFamily = await _getFosterFamilyUseCase.Execute(validationResult.ConflictingFosterCarerId, false)
                    };
                    return View("Resolve_Partner_Conflict_FF", conflictViewModel);
                }

                return RedirectToAction("Enter_Partner_Details_FF", new { request.ContextId });
            }

            // Update session context with the FosterPartnerDetailsViewModel post successful validation
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
        public async Task<IActionResult> Update_Partner_Details_FF(Guid fosterCarerId)
        {
            // Load foster carer record to ensure it exists and we have access
            var request = await _getFosterFamilyUseCase.Execute(fosterCarerId);

            // Clear session to restart journey if required
            DetectJourneyRestartAndClearSession(fosterCarerId.ToString(), "FosterPartnerDetails", "UpdatePartner");

            // Pull the FosterPartnerDetailsViewModel from session if it exists    
            var viewModel = _sessionContextService.GetSessionData<FosterPartnerDetailsViewModel>(fosterCarerId.ToString(), "FosterPartnerDetails");

            if (viewModel != null)
            {
                // Validate the model if it exists to allow post-redirect-get behaviour for validation messages
                TryValidateModel(viewModel);
                await _validateFosterPartnerDetailsUseCase.Execute(viewModel, ModelState);
            }
            else
            {
                // Otherwise load the view model from the use case if it does not exist in session
                viewModel = await _loadFosterPartnerDetailsUseCase.Execute(request);
            }

            return View("Enter_Partner_Details_FF", viewModel);
        }

        [HttpPost("UpdatePartner")]
        public async Task<IActionResult> Update_Partner_Details_FF(FosterPartnerDetailsViewModel request)
        {

            // Load foster carer record to ensure it exists and we have access
            var existingCarer = await _getFosterFamilyUseCase.Execute(request.FosterCarerId, true);
            
            // Populate session context with the FosterPartnerDetailsViewModel before validation
            _sessionContextService.SetSessionData(request.FosterCarerId.ToString(), "FosterPartnerDetails", request);

            var validationResult = await _validateFosterPartnerDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                if (validationResult != null && validationResult.ConflictingFosterCarerId != Guid.Empty)
                {
                    var conflictViewModel = new FosterPartnerConflictViewModel()
                    {
                        PartnerDetails = request,
                        ConflictingFamily = await _getFosterFamilyUseCase.Execute(validationResult.ConflictingFosterCarerId, false)
                    };
                    return View("Resolve_Partner_Conflict_FF", conflictViewModel);
                }

                // Redirect back to get page to show validation errors
                return RedirectToAction("Update_Partner_Details_FF", new { request.FosterCarerId });
            }

            UpdateFosterCarerRequest updateRequest = new()
            {
                FosterCarerRequest = new FosterCarerRequest
                {
                    CarerFirstName = existingCarer.CarerFirstName,
                    CarerLastName = existingCarer.CarerLastName,
                    CarerDateOfBirth = existingCarer.CarerDateOfBirth,
                    CarerNationalInsuranceNumber = existingCarer.CarerNationalInsuranceNumber,
                    HasPartner = true
                },
                FosterPartnerRequest = request.BuildRequest()
            };

            await _updateFosterCarerUseCase.Execute(request.FosterCarerId, updateRequest);

            // Clear partner details from session
            _sessionContextService.ClearSessionData(request.FosterCarerId.ToString(), "FosterPartnerDetails");

            return RedirectToAction("Family_Record_FF", new
            {
                request.FosterCarerId,
                Confirmation = existingCarer.HasPartner ?
                    "Changes to partner saved" :
                    "Partner added"
            });
        }

        [HttpGet("RemovePartner/{FosterCarerId}")]
        public async Task<IActionResult> Remove_Partner_Details_FF(Guid fosterCarerId)
        {
            var request = await _getFosterFamilyUseCase.Execute(fosterCarerId);
            var viewModel = await _loadFosterPartnerDetailsUseCase.Execute(request);
            return View("Remove_Partner_Details_FF", viewModel);
        }

        [HttpPost("RemovePartner")]
        public async Task<IActionResult> Remove_Partner_Details_FF(FosterPartnerDetailsViewModel request)
        {
            // Load foster carer record to ensure it exists and we have access
            var response = await _getFosterFamilyUseCase.Execute(request.FosterCarerId, true);

            // Build request
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
            await _updateFosterCarerUseCase.Execute(request.FosterCarerId, updateRequest);

            // Ensure any cached partner details are cleared from session
            _sessionContextService.ClearSessionData(request.FosterCarerId.ToString(), "FosterPartnerDetails");

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
            var viewModel = _sessionContextService.GetSessionData<FosterChildDetailsViewModel>(contextId, "FosterChildDetails");

            // Validate the model if it exists to allow post-redirect-get behaviour for validation messages
            if (viewModel != null)
            {
                TryValidateModel(viewModel);
                await _validateFosterChildDetailsUseCase.Execute(viewModel, ModelState);
            }

            viewModel ??= new FosterChildDetailsViewModel
            {
                ContextId = contextId,
                HasPartner = fosterCarerDetails.HasPartner
            };

            return View(viewModel);
        }

        [HttpPost("EnterChild")]
        public async Task<IActionResult> Enter_Child_Details_FF(FosterChildDetailsViewModel request)
        {
            // Populate session context with the FosterCarerDetailsViewModel before validation
            _sessionContextService.SetSessionData(request.ContextId, "FosterChildDetails", request);

            var validationResult = await _validateFosterChildDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return RedirectToAction("Enter_Child_Details_FF", new { request.ContextId });
            }

            // Update session context with the FosterChildDetailsViewModel post successful validation
            _sessionContextService.SetSessionData(request.ContextId, "FosterChildDetails", request);

            // Redirect to enter submitted date details if they do not yet exist
            var submittedDateDetails = _sessionContextService.GetSessionData<FosterApplicationSubmittedDateViewModel>(request.ContextId, "FosterApplicationSubmittedDate");
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

        [HttpGet("UpdateChild/{fosterChildId}")]
        public async Task<IActionResult> Update_Child_Details_FF(Guid fosterChildId)
        {
            // Load foster child record to ensure it exists and we have access
            var request = await _getFosterChildUseCase.Execute(fosterChildId);

            // Clear session to restart journey if required
            DetectJourneyRestartAndClearSession(fosterChildId.ToString(), "FosterChildDetails", "UpdateChild");

            // Pull the FosterChildDetailsViewModel from session if it exists    
            var viewModel = _sessionContextService.GetSessionData<FosterChildDetailsViewModel>(fosterChildId.ToString(), "FosterChildDetails");

            if (viewModel != null)
            {
                // Validate the model if it exists to allow post-redirect-get behaviour for validation messages
                TryValidateModel(viewModel);
                await _validateFosterChildDetailsUseCase.Execute(viewModel, ModelState);
            }
            else
            {
                // Otherwise load the view model from the use case if it does not exist in session
                viewModel = await _loadFosterChildDetailsUseCase.Execute(request);
            }

            return View("Enter_Child_Details_FF", viewModel);
        }


        [HttpPost("UpdateChild")]
        public async Task<IActionResult> Update_Child_Details_FF(FosterChildDetailsViewModel request)
        {
            // Load foster child record to ensure it exists and we have access
            await _getFosterChildUseCase.Execute(request.FosterChildId, true);

            // Populate session context with the FosterChildDetailsViewModel before validation
            _sessionContextService.SetSessionData(request.FosterChildId.ToString(), "FosterChildDetails", request);

            var validationResult = await _validateFosterChildDetailsUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return RedirectToAction("Update_Child_Details_FF", new { request.FosterChildId });
            }

            UpdateFosterChildRequest updateRequest = new()
            {
                FosterChildRequest = request.BuildRequest()
            };

            await _updateFosterChildUseCase.Execute(request.FosterChildId, updateRequest);

            // Clear child details from session
            _sessionContextService.ClearSessionData(request.FosterChildId.ToString(), "FosterChildDetails");

            return RedirectToAction("Code_Record_FF", new { request.FosterChildId, Confirmation = "Changes to child saved" });
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

            // Pull the fosterChildDetailsViewModel from session if it exists
            var viewModel = _sessionContextService.GetSessionData<FosterApplicationSubmittedDateViewModel>(contextId, "FosterApplicationSubmittedDate");

            // Validate the model if it exists to allow post-redirect-get behaviour for validation messages
            if (viewModel != null)
            {
                TryValidateModel(viewModel);
                await _validateFosterApplicationSubmittedDateUseCase.Execute(viewModel, ModelState);
            }

            viewModel ??= new FosterApplicationSubmittedDateViewModel { ContextId = contextId };

            return View(viewModel);
        }

        [HttpPost("SubmittedDate")]
        public async Task<IActionResult> Enter_Submitted_Date_Details_FF(FosterApplicationSubmittedDateViewModel request)
        {
            // Populate session context with the FosterCarerDetailsViewModel before validation
            _sessionContextService.SetSessionData(request.ContextId, "FosterApplicationSubmittedDate", request);

            var validationResult = await _validateFosterApplicationSubmittedDateUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return RedirectToAction("Enter_Submitted_Date_Details_FF", new { request.ContextId });
            }

            // Update session context with the FosterApplicationSubmittedDateViewModel post successful validation
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

            // Build request
            var fosterFamilyRequest = new FosterFamilyRequest
            {
                FosterCarer = fosterCarerDetails.BuildRequest(),
                FosterChild = fosterChildDetails.BuildRequest(),
                Partner = fosterPartnerDetails?.BuildRequest(),
                HasPartner = fosterCarerDetails.HasPartner,
                SubmissionDate = fosterApplicationSubmittedDate.SubmissionDate
            };
            fosterFamilyRequest.FosterCarer.LocalAuthorityID = GetLocalAuthorityId();

            // Get foster code preview
            var fosterCodePreview = await _previewFosterCodeUseCase.Execute(fosterFamilyRequest);

            // Build view model
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

            // Prepare request
            var fosterFamilyRequest = new FosterFamilyRequest
            {
                FosterCarer = fosterCarerDetails.BuildRequest(),
                FosterChild = fosterChildDetails.BuildRequest(),
                Partner = fosterPartnerDetails?.BuildRequest(),
                HasPartner = fosterCarerDetails.HasPartner,
                SubmissionDate = fosterApplicationSubmittedDate.SubmissionDate,
            };
            fosterFamilyRequest.FosterCarer.LocalAuthorityID = GetLocalAuthorityId();

            try
            {
                var response = await _createFosterFamilyUseCase.Execute(fosterFamilyRequest);
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

        [HttpGet("CodeCreated/{fosterChildId}")]
        public async Task<IActionResult> Code_Created_FF(Guid fosterChildId)
        {
            var response = await _getFosterChildUseCase.Execute(fosterChildId, true);
            var viewModel = new FosterFamilyCreatedViewModel { Response = response };
            return View(viewModel);
        }

        [HttpGet("Family/{FosterCarerId}")]
        public async Task<IActionResult> Family_Record_FF(Guid fosterCarerId, string? confirmation)
        {
            var response = await _getFosterFamilyUseCase.Execute(fosterCarerId, true);
            var viewModel = new FosterFamilyViewModel()
            {
                Response = response,
                Confirmation = confirmation
            };
            return View(viewModel);
        }

        [HttpGet("Code/{fosterChildId}")]
        public async Task<IActionResult> Code_Record_FF(Guid fosterChildId, string? confirmation)
        {
            var childResponse = await _getFosterChildUseCase.Execute(fosterChildId, true);
            var viewModel = new FosterFamiliesCodeResponseViewModel()
            {
                Response = childResponse,
                CodeProperties = new EligibilityCodeProperties(childResponse),
                Confirmation = confirmation
            };
            return View(viewModel);
        }


        [HttpGet("EnterReconfirmDate/{fosterChildId}")]
        public async Task<IActionResult> Enter_Reconfirm_Date_FF(Guid fosterChildId)
        {
            // Get child record to ensure it exists before displaying the reconfirmation form
            var childResponse = await _getFosterChildUseCase.Execute(fosterChildId, false);

            // Pull the FosterCodeReconfirmDateViewModel from session by FosterChildId if it exists
            var fosterCodeReconfirmDateViewModel = _sessionContextService.GetSessionData<FosterCodeReconfirmDateViewModel>(fosterChildId.ToString(), "FosterCodeReconfirmDate");
            // If fosterCodeReconfirmDateViewModel is null, create a new instance with the FosterChildId
            if (fosterCodeReconfirmDateViewModel == null)
            {
                fosterCodeReconfirmDateViewModel = new FosterCodeReconfirmDateViewModel { FosterChildId = fosterChildId };
            }
            return View("Enter_Reconfirm_Date_Details_FF", fosterCodeReconfirmDateViewModel);
        }

        [HttpPost("EnterReconfirmDate")]
        public async Task<IActionResult> Enter_Reconfirm_Date_FF(FosterCodeReconfirmDateViewModel request)
        {
            var validationResult = _validateFosterCodeReconfirmDateUseCase.Execute(request, ModelState);
            if (validationResult == null || !validationResult.IsValid)
            {
                return View("Enter_Reconfirm_Date_Details_FF", request);
            }
            _sessionContextService.SetSessionData(request.FosterChildId.ToString(), "FosterCodeReconfirmDate", request);

            return RedirectToAction("Check_Reconfirm_Details_FF", new { request.FosterChildId });
        }

        [HttpGet("CheckReconfirmDetails/{fosterChildId}")]
        public async Task<IActionResult> Check_Reconfirm_Details_FF(Guid fosterChildId)
        {
            // Pull the FosterCodeReconfirmDateViewModel from session by FosterChildId if it exists
            var fosterCodeReconfirmDateViewModel = _sessionContextService.GetSessionData<FosterCodeReconfirmDateViewModel>(fosterChildId.ToString(), "FosterCodeReconfirmDate");
            // If fosterCodeReconfirmDateViewModel is null, redirect to enter reconfirm date to complete required details
            if (fosterCodeReconfirmDateViewModel == null) { return RedirectToAction("Enter_Reconfirm_Date_FF", new { fosterChildId }); }

            var childResponse = await _getFosterChildUseCase.Execute(fosterChildId, true);
            var viewModel = new FosterCodeReconfirmCheckDetailsViewModel()
            {
                FosterChildId = fosterChildId,
                ChildFullName = $"{childResponse.ChildFirstName} {childResponse.ChildLastName}",
                FosterCodeReconfirmDateViewModel = fosterCodeReconfirmDateViewModel
            };

            var previewResult = await _previewFosterCodeReconfirmUseCase.Execute(
                fosterChildId,
                new FosterChildReconfirmRequest
                {
                    SubmissionDate = fosterCodeReconfirmDateViewModel.SubmissionDate,
                    EligibilityCode = childResponse.EligibilityCode
                }
            );
            viewModel.FosterCodePreview = previewResult;
            return View("Check_Reconfirm_Details_FF", viewModel);
        }


        [HttpPost("CheckReconfirmDetails")]
        public async Task<IActionResult> Check_Reconfirm_Details_FF(FosterCodeReconfirmCheckDetailsViewModel request)
        {
            // Pull the FosterCodeReconfirmDateViewModel from session by FosterChildId if it exists
            var fosterCodeReconfirmDateViewModel = _sessionContextService.GetSessionData<FosterCodeReconfirmDateViewModel>(request.FosterChildId.ToString(), "FosterCodeReconfirmDate");
            // If fosterCodeReconfirmDateViewModel is null, redirect to enter reconfirm date to complete required details
            if (fosterCodeReconfirmDateViewModel == null) { return RedirectToAction("Enter_Reconfirm_Date_FF", new { request.FosterChildId }); }

            // Get child record to ensure it exists and to retrieve the eligibility code for reconfirmation
            var childResponse = await _getFosterChildUseCase.Execute(fosterCodeReconfirmDateViewModel.FosterChildId, true);

            // Reconfirm the foster child's eligibility code
            var reconfirmResponse = await _reconfirmFosterCodeUseCase.Execute(
                fosterCodeReconfirmDateViewModel.FosterChildId,
                new FosterChildReconfirmRequest
                {
                    SubmissionDate = fosterCodeReconfirmDateViewModel.SubmissionDate,
                    EligibilityCode = childResponse.EligibilityCode
                }
            );

            // Build view model for the reconfirmation result
            var viewModel = new FosterCodeReconfirmedViewModel() { Response = reconfirmResponse };
            return View("Code_Reconfirmed_FF", viewModel);
        }
    }
}
