namespace CheckChildcareEligibility.Admin.Boundary.Requests
{
    public class CheckEligibilityRequestData : CheckEligibilityRequestDataBase
    {
        public string LastName { get; set; } = string.Empty;
    }
    public class CheckEligibilityRequestWorkingFamiliesData : CheckEligibilityRequestDataBase
    {
       public string EligibilityCode { get; set; } = string.Empty;
    }
}
