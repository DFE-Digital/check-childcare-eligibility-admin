using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Domain.Constants.Generic
{
    internal class BulkCheckTags
    {
        public string Description { get; set; }
        public string Colour { get; set; }
    }

    internal static class BulkCheckConstants
    {
        public static readonly Dictionary<string, BulkCheckTags> StatusTags = new()
        {
            [BulkCheckStatus.InProgress.ToString()] = new()
            {
                Description = "In Progress",
                Colour = "govuk-tag--blue"
            },

            [BulkCheckStatus.Completed.ToString()] = new()
            {
                Description = "Completed",
                Colour = "govuk-tag--green"
            },

            [BulkCheckStatus.Failed.ToString()] = new()
            {
                Description = "Failed",
                Colour = "govuk-tag--red"
            },

            [BulkCheckStatus.ApplicationCreationInProgress.ToString()] = new()
            {
                Description = "Application creation in progress",
                Colour = "govuk-tag--blue"
            },

            [BulkCheckStatus.ApplicationsCreated.ToString()] = new()
            {
                Description = "Applications created",
                Colour = "govuk-tag--green"
            },

            [BulkCheckStatus.ApplicationCreationFailed.ToString()] = new()
            {
                Description = "Application creation failed",
                Colour = "govuk-tag--failed"
            }
        };
    }
}