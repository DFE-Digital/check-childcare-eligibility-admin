// Ignore Spelling: Fsm

using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class ApplicationStatusUpdateRequest
{
    public ApplicationStatusData? Data { get; set; }
}

public class ApplicationStatusData
{
    public ApplicationStatus Status { get; set; }
}