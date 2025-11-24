using CheckChildcareEligibility.Admin.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Controllers;

public class HomeController : BaseController
{
    public IActionResult Index()
    {
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

        // Check if the organization is a Local Authority
        if (_Claims?.Organisation?.Category?.Name == null ||
            !_Claims.Organisation.Category.Name.Equals("Local Authority", StringComparison.OrdinalIgnoreCase))
        {
            return View("UnauthorizedOrganization");
        }

        return View(_Claims);
    }

    //Single

    [HttpGet("MenuSingleCheck")]
    public IActionResult MenuSingleCheck(bool clearData = false)
    {
        // If clearData is true, remove the ParentDetails from TempData
        if (clearData)
        {
            TempData.Remove("ParentDetails");
            TempData.Remove("ParentAndChildDetails");
            TempData.Remove("Errors");
        }
        return View("MenuSingleCheck");
    }


    [HttpPost("MenuSingleCheck")]
    public IActionResult MenuSingleCheck([FromForm] string eligibilityType)
    {
        if (string.IsNullOrEmpty(eligibilityType))
        {
            return BadRequest("Eligibility type is required.");
        }

        TempData["eligibilityType"] = eligibilityType;

        if(eligibilityType.Equals("WF"))
        {
            return RedirectToAction("Enter_Details_WF", "WorkingFamiliesCheck");
        }

        return RedirectToAction("Enter_Details", "Check");
    }

    //Bulk
    public IActionResult MenuBulkCheck()
    {
        return View("MenuBulkCheck");
    }

    [HttpPost]
    public IActionResult MenuBulkCheck([FromForm] string eligibilityType)
    {
        if (string.IsNullOrEmpty(eligibilityType))
        {
            return BadRequest("Eligibility type is required.");
        }

        TempData["eligibilityType"] = eligibilityType;
        TempData["JustUploaded"] = "";

        return RedirectToAction("Bulk_Check", "BulkCheck");
    }

    public IActionResult Accessibility()
    {
        return View("Accessibility");
    }

    public IActionResult Cookies()
    {
        return View("Cookies");
    }

    public IActionResult Guidance()
    {
        return View("Guidance");
    }

    public IActionResult FSMFormDownload()
    {
        return View("FSMFormDownload");
    }
}