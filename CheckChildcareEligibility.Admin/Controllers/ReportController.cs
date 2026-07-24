using CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeConstants;
using CheckChildcareEligibility.Admin.Gateways;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IMenuProvider _menuProvider;
        public ReportController(
             IMenuProvider menuProvider,
            IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
        {
            _menuProvider = menuProvider;
        }
        public async Task<IActionResult> Reports()
        {
            await GetDfeClaimsAsync();
            ViewBag.Claims = _Claims;
            var menu = _menuProvider.GetMenuItemsForReports(_Claims);
            return View(menu);
        }
    }
}
