namespace CheckChildcareEligibility.Admin.Domain.Enums;

public enum CheckEligibilityStatus
{
    queuedForProcessing,
    parentNotFound,
    eligible,
    notEligible,
    error
}