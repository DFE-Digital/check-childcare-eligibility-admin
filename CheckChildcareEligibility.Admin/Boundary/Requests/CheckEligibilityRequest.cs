using CheckChildcareEligibility.Admin.Domain.Enums;

namespace CheckChildcareEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestDataBase : IEligibilityServiceType
{
    public CheckEligibilityType baseType;
    public int? Sequence { get; set; }
}

public interface IEligibilityServiceType
{
}

#region FreeSchoolMeals Type

public class CheckEligibilityRequestData_Fsm : CheckEligibilityRequestDataBase
{
    public CheckEligibilityRequestData_Fsm(CheckEligibilityType eligibilityType)
    {
        baseType = eligibilityType;
    }

    public string? NationalInsuranceNumber { get; set; }

    public string LastName { get; set; }

    public string DateOfBirth { get; set; }

    public string? NationalAsylumSeekerServiceNumber { get; set; }
}

public class CheckEligibilityRequest_Fsm
{
    public CheckEligibilityRequestData_Fsm? Data { get; set; }
}

public class CheckEligibilityRequestBulk_Fsm
{
    public IEnumerable<CheckEligibilityRequestData_Fsm> Data { get; set; }
}

#endregion