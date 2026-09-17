using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;
using CheckChildcareEligibility.Admin.Helpers;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels;

public class FosterFamiliesCodeResponseViewModel
{
    public FosterChildResponse Response { get; set; }

    public EligibilityCodeProperties CodeProperties { get; set; }

    public Term ValidFromTerm => CodeProperties.CurrentTerm.Name != TermName.None ? CodeProperties.CurrentTerm : CodeProperties.NextTerm;

    public string CodeStatus => WorkingFamiliesCheckHelper.GetCodeStatus_FF(CodeProperties);

    public string[] GetReconfirmationStatus()
    {
        if (CodeProperties.ChildIsTooOld)
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusChildTooOld;
        }
        else if (DateTime.Now < Response.ReconfirmationProperties.StartDate)
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusNotDueYet;
        }
        else if (DateTime.Now >= Response.ReconfirmationProperties.StartDate && DateTime.Now <= Response.ReconfirmationProperties.EndDate) //due now
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusDueNow;
        }
        else if (DateTime.Now > Response.ReconfirmationProperties.EndDate) //overdue - Needs reconfirming now
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusOverdue;
        }
        return ["Not set", "purple"]; // Should not reach this
    }

}