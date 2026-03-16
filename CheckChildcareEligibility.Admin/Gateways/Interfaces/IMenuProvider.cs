using CheckChildcareEligibility.Admin.Domain.DfeSignIn;
using CheckChildcareEligibility.Admin.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface IMenuProvider
{
    IEnumerable<MenuItem> GetMenuItemsFor(DfeClaims claims);
}

public class MenuProvider : IMenuProvider
{
    private readonly IMemoryCache _cache;
    public MenuProvider(IMemoryCache cache) => _cache = cache;

    public IEnumerable<MenuItem> GetMenuItemsFor(DfeClaims claims)
    {
        if (claims == null || !claims.Roles.Any())
        {
            return Array.Empty<MenuItem>();
        }
        var role = claims.Roles[0].Code;

        return _cache.GetOrCreate($"Menu_{role}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return BuildMenuForRole(role);
        });
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
                        "Run a check for one parent or guardian",
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
}