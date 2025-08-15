using CheckChildcareEligibility.Admin.Domain.Enums;
using System;
using System.Linq;

namespace CheckChildcareEligibility.Admin.Boundary.Requests
{
    public class CheckEligibilityRequestData : CheckEligibilityRequestDataBase
    {
        public CheckEligibilityRequestData(CheckEligibilityType eligibilityType)
        {
            baseType = eligibilityType;
        }

        public string LastName { get; set; } = string.Empty;

        public string DateOfBirth { get; set; } = string.Empty;
        public string ChildDateOfBirth { get; set; } = string.Empty;
        public string EligibilityCode { get; set; } = string.Empty;

        public string NationalInsuranceNumber { get; set; } = string.Empty;

    }
}
