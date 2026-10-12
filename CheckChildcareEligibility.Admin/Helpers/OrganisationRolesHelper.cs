using System.Security.Claims;

namespace CheckChildcareEligibility.Admin.Helpers
{
    public static class OrganisationRolesHelper
    {
        public static bool IsLocalAuthorityPrivateBeta(string betaList, string localAuthorityId)
        { 

            var betaListArray = betaList?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];

            return betaListArray.Contains(localAuthorityId, StringComparer.OrdinalIgnoreCase);

        }
    }
}
