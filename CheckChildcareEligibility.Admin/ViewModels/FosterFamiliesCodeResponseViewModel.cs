using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Helpers;

namespace CheckChildcareEligibility.Admin.ViewModels;

public class FosterFamiliesCodeResponseViewModel
{
    public FosterChildResponse Response { get; set; }

    public bool ChildIsTooYoung => WorkingFamiliesCheckHelper.ChildIsTooYoung(Response.ChildDateOfBirth, Response.ValidityStartDate);
    public bool ChildIsTooOld => WorkingFamiliesCheckHelper.ChildIsTooOld(Response.ChildDateOfBirth, DateTime.UtcNow.Date);

    public string[] GetReconfirmationStatus()
    {
        if (ChildIsTooOld)
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusChildTooOld;
        }
        else if (DateTime.Now < Response.ReconfirmBetweenStart)
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusNotDueYet;
        }
        else if (DateTime.Now >= Response.ReconfirmBetweenStart && DateTime.Now <= Response.ReconfirmBetweenEnd) //due now
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusDueNow;
        }
        else if (DateTime.Now > Response.ReconfirmBetweenEnd) //overdue - Needs reconfirming now
        {
            return WorkingFamiliesResponseDetails.ReconfirmationStatusOverdue;
        }
        return ["Not set", "purple"]; // Should not reach this
    }

}