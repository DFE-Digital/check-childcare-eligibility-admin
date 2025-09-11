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
}

public class CheckEligibilityItemResponse : CheckEligibilityItemResponseBase
{
    public CheckEligibilityItem Data { get; set; }
}
#endregion

#region Working Families
public class CheckEligibilityItemWorkingFamilies : CheckEligibilityItem
{

    public string EligibilityCode { get; set; }
    public string ValidityStartDate { get; set; }
    public string ValidityEndDate { get; set; }
    public string GracePeriodEndDate { get; set; }
}
public class CheckEligibilityItemWorkingFamiliesResponse : CheckEligibilityItemResponseBase
{
    public CheckEligibilityItemWorkingFamilies Data { get; set; }
}
#endregion