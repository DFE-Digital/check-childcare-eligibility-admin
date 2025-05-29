using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Boundary.Requests;
#region FreeSchoolMeals Type

public class CheckEligibilityRequestDataV1 : CheckEligibilityRequestDataBase
{
    public CheckEligibilityRequestDataV1(CheckEligibilityType eligibilityType)
    {
        baseType = eligibilityType;
    }

    public string? NationalInsuranceNumber { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string DateOfBirth { get; set; } = string.Empty;

    public string? NationalAsylumSeekerServiceNumber { get; set; }
}

#endregion