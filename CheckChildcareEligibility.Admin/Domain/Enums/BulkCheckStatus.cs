using System.ComponentModel;

namespace CheckChildcareEligibility.Admin.Domain.Enums
{
    public enum BulkCheckStatus
    {
        InProgress,
        Completed,
        Failed,
        Cancelled,
        Deleted,
        ApplicationCreationInProgress,
        ApplicationsCreated,
        ApplicationCreationFailed

    }
}