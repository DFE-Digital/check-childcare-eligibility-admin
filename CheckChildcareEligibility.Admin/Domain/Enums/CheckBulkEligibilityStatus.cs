using System.ComponentModel;

namespace CheckChildcareEligibility.Admin.Domain.Enums
{
    public enum CheckBulkEligibilityStatus
    {
        [Description("Not started")] NotStarted,
        [Description("In progress")] InProgress,
        [Description("Completed")] Completed,
    }
}
