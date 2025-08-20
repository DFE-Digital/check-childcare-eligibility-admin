namespace CheckChildcareEligibility.Admin.Domain.Enums;

public enum CheckEligibilityStatus
{
    queuedForProcessing,
    parentNotFound,
    notFound,
    eligible,
    notEligible,
    error
}