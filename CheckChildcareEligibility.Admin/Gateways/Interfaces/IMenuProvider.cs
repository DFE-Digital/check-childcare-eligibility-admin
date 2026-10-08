using CheckChildcareEligibility.Admin.Controllers;
using CheckChildcareEligibility.Admin.Domain.Constants;
using CheckChildcareEligibility.Admin.Domain.DfeSignIn;
using CheckChildcareEligibility.Admin.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.FeatureManagement;
using System.Security.Claims;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface IMenuProvider
{
    Task<IEnumerable<MenuItem>> GetMenuItemsFor(DfeClaims claims);
    IEnumerable<MenuItem> GetMenuItemsForReports(DfeClaims claims);
}

public class MenuProvider : IMenuProvider
{
    private readonly IMemoryCache _cache;
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _config;

    public MenuProvider(IMemoryCache cache, IFeatureManager featureManager, IConfiguration config)
    {
        _cache = cache;
        _featureManager = featureManager;
        _config = config;
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsFor(DfeClaims claims)
    {
        if (claims == null || !claims.Roles.Any())
        {
            return Array.Empty<MenuItem>();
        }
        var role = claims.Roles[0].Code;
        var cacheKey = $"Menu_{role}";
        var cacheHit = _cache.TryGetValue(cacheKey, out List<MenuItem>? cachedMenu);
        if (cacheHit && cachedMenu is not null)
        {
            return cachedMenu;
        }
        var menu = await FilterTilesAsync(BuildMenuForRole(role));
        _cache.Set(cacheKey, menu, TimeSpan.FromMinutes(5));
        return menu;
    }

    private async Task<List<MenuItem>> FilterTilesAsync(IEnumerable<MenuItem> items)
    {
        var result = new List<MenuItem>();

        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.FeatureName))
            {
                if (!await _featureManager.IsEnabledAsync(item.FeatureName))
                    continue; // hide tile
            }

            result.Add(item);
        }

        return result;
    }

    private IEnumerable<MenuItem> BuildMenuForRole(string role)
    {
        switch (role)
        {
            case "mefcsLocalAuthority":
                return new[] {
                    new MenuItem(
                        "Home",
                        "Home",
                        "Dashboard",
                        "Home",
                        ""
                        ),
                    new MenuItem(
                        "Run a check",
                        "Run a check",
                        "Run an eligibility check for one parent or guardian.",
                        "Home",
                        "MenuSingleCheck"
                        ),
                    new MenuItem(
                        "Run batch check",
                        "Run a batch check",
                        "Run an eligibility check for multiple parents or guardians.",
                        "Home",
                        "MenuBulkCheck"
                        ),
                    new MenuItem(
                        "Manage foster families",
                        "Manage foster families",
                        "Manage foster families claiming Childcare for working families.",
                        "FosterFamilies",
                        "Search",
                        featureName: Features.FosterFamilies
                        ),
                    new MenuItem(
                        "Run reports",
                        "Run reports",
                        "Run and export reports on all applications for childcare.",
                        "Report",
                        "Reports",
                        featureName: Features.Reports
                        ),
                    new MenuItem(
                        "Guidance",
                        "Guidance",
                        "Read guidance on running eligibility checks and managing foster families.",
                        "Home",
                        "GuidanceHome"
                        )
                };
            default: return Enumerable.Empty<MenuItem>();
        }
    }
    public IEnumerable<MenuItem> GetMenuItemsForReports(DfeClaims claims)
    {
        if (!IsLocalAuthorityPrivateBeta(claims))
        {
            return Enumerable.Empty<MenuItem>();
        }
        return BuildMenuForRoleReports();
    }
    private IEnumerable<MenuItem> BuildMenuForRoleReports()
    {
        return new[] {
            new MenuItem(
            "View eligibility code history",
            "View eligibility code history",
            "View the event listing for a code, showing application and reconfirmation history",
            "Report",
            "Code_Search"
            )
        };
    }
    public bool IsLocalAuthorityPrivateBeta(DfeClaims claims)

    {
        var laId = int.Parse(claims.Organisation.EstablishmentNumber);
        var allowedLAs = _config
        .GetSection("FeatureFlags:LAsThatCanUseWF")
        .Get<int[]>() ?? Array.Empty<int>();
        return allowedLAs.Contains(laId);

    }

}