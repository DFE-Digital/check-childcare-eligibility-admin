using CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeConstants;
using CheckChildcareEligibility.Admin.Gateways;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Controllers
{
    public class ManageFosterFamiliesController : BaseController
    {
        private readonly IMenuProvider _menuProvider;
        public ManageFosterFamiliesController(
            IMenuProvider menuProvider,
            IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
        {
            _menuProvider = menuProvider;
        }

        public IActionResult ManageFosterFamilies()
        {
            return View();
        }
    }
}
