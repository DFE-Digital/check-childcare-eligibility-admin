using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;

namespace CheckChildcareEligibility.Admin.Boundary.Responses;


public class CheckEligibilityItemResponseBase
{
    public CheckEligibilityResponseLinks Links { get; set; }
}

#region 2YO EYPP
public class CheckEligibilityItem
{
    public string NationalInsuranceNumber { get; set; }

    public string LastName { get; set; }

    public string DateOfBirth { get; set; }

    public string Status { get; set; }

    public DateTime Created { get; set; }

    public int? Order { get; set; }
}

public class CheckEligibilityItemResponse : CheckEligibilityItemResponseBase
{
    public CheckEligibilityItem Data { get; set; }
}
#endregion

#region Working Families
public class CheckEligibilityItemWorkingFamilies
{
    public TermValidity? TermValidity { get; set; }
    public ReconfirmationProperties? ReconfirmationProperties { get; set; }
    public bool? IsDiscretionaryValidityStartDateApplied { get; set; }
    public EligibilityCodeType? EligibilityCodeType { get; set; }
    public string NationalInsuranceNumber { get; set; }
    public string LastName { get; set; }
    public string DateOfBirth { get; set; }
    public string Status { get; set; }
    public DateTime Created { get; set; }
    public string EligibilityCode { get; set; }
    public string ValidityStartDate { get; set; }
    public string ValidityEndDate { get; set; }
    public string GracePeriodEndDate { get; set; }
    public int? Order { get; set; }
}
public class TermValidity
{

    public TermName? Current { get; set; }
    public TermName? Next { get; set; }

    public TermValidity(TermName? current, TermName? next)
    {
        Current = current ?? TermName.None;
        Next = next ?? TermName.None;
    }
}

public class ReconfirmationProperties
{

    public string? StartDate { get; set; }

    public string? EndDate { get; set; }

    public ReconfirmationStatus Status { get; set; }

}
public class CheckEligibilityItemWorkingFamiliesResponse : CheckEligibilityItemResponseBase
{
    public CheckEligibilityItemWorkingFamilies Data { get; set; }
}
#endregion