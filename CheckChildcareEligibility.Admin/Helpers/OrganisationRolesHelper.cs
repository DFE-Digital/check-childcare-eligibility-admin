namespace CheckChildcareEligibility.Admin.Helpers
{
    public static class OrganisationRolesHelper
    {
        public static bool IsWorkingFamiliesEnabledForLA(string betaList, string localAuthorityId)
        {
            // if list is empty feature is enabled for all
            if (String.IsNullOrEmpty(betaList)) return true;

            var betaListArray = betaList?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
            return betaListArray.Contains(localAuthorityId, StringComparer.OrdinalIgnoreCase);

        }
    }
}