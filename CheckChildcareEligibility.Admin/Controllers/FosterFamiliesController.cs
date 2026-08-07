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
    public class FosterFamiliesController : BaseController
    {
        private readonly IMenuProvider _menuProvider;
        private readonly ISearchFosterFamiliesRecordsUseCase _searchFosterFamiliesRecordsUseCase;

        public FosterFamiliesController(
            IMenuProvider menuProvider,
            ISearchFosterFamiliesRecordsUseCase searchFosterFamiliesRecordsUseCase,
            ICreateFosterFamilyUseCase performCreateFosterFamilyUseCase,
            IValidateFosterFamilyUseCase validateFosterFamilyUseCase,
            IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
        {
            _menuProvider = menuProvider;
            _searchFosterFamiliesRecordsUseCase = searchFosterFamiliesRecordsUseCase;
        }

        public async Task<IActionResult> SearchFosterFamiliesRecords(int pageNumber = 1)
        {
            var fosterFamiliesSearchRequest = new FosterFamiliesSearchRequest(pageNumber, 10);
            var response = await _searchFosterFamiliesRecordsUseCase.Execute(fosterFamiliesSearchRequest);

            FosterFamilyViewModel vm = new FosterFamilyViewModel
            {
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalNumberOfRecords = response.TotalNumberOfRecords,
                Data = response.Data
            };
            return View(vm);
        }
    }
}
