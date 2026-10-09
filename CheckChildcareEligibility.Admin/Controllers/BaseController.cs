using CheckChildcareEligibility.Admin.Domain.DfeSignIn;
using CheckChildcareEligibility.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace CheckChildcareEligibility.Admin.Controllers;

[Authorize]
public class BaseController : Controller
{
    protected DfeClaims? _Claims;

    private readonly IDfeSignInApiService _dfeSignInApiService;
    private readonly IConfiguration _config;

    public BaseController(
        IDfeSignInApiService dfeSignInApiService,
        IConfiguration configuration)
    {
        _dfeSignInApiService = dfeSignInApiService;
        _config = configuration;
    }

    public async Task GetDfeClaimsAsync()
    {
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

        // Fetch roles from DfE Sign-in API
        if (_Claims.Organisation.Id != Guid.Empty && !string.IsNullOrEmpty(_Claims.User?.Id))
        {
            _Claims.Roles = await _dfeSignInApiService.GetUserRolesAsync(_Claims.User.Id, _Claims.Organisation.Id);
        }
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await GetDfeClaimsAsync();
        ViewBag.Claims = _Claims;
        await base.OnActionExecutionAsync(context, next);
    }

    internal int GetLocalAuthorityId()
    {
        return int.Parse(_Claims.Organisation.EstablishmentNumber);
    }

     public bool IsLocalAuthorityPrivateBeta()
    {
        var localAuthorityId = _Claims.Organisation.EstablishmentNumber?.Trim();

        var betaList = _config.GetSection<string[]> ("FeatureFlags:LAsThatCanUseWF");
     
         return betaList?.Contains(localAuthorityId, StringComparer.OrdinalIgnoreCase)?? false;
        
    }
    {
        var localAuthorityId = _Claims.Organisation.EstablishmentNumber?.Trim();

        var allowedLASection = _config.GetSection("FeatureFlags:LAsThatCanUseWF");
        var allowedLAs = allowedLASection.Get<string[]>() ?? Array.Empty<string>();

        if (!allowedLAs.Any())
        {
            var allowedLASetting = allowedLASection.Get<string>();

            allowedLAs = string.IsNullOrWhiteSpace(allowedLASetting)
            ? Array.Empty<string>()
            : allowedLASetting.Split(',')
            .Select(x => x.Trim())
            .ToArray();
        }

        return allowedLAs.Contains(localAuthorityId, StringComparer.OrdinalIgnoreCase);
    }
}