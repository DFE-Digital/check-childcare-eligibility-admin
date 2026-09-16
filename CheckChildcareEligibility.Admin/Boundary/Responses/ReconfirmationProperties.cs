using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;

namespace CheckChildcareEligibility.Admin.Boundary.Responses;

public class ReconfirmationProperties
{

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public ReconfirmationStatus Status { get; set; }

}