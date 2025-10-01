using CheckChildcareEligibility.Admin.Models;
using System.Security.Policy;

namespace CheckChildcareEligibility.Admin.Domain.Constants.Generic
{
    public static class WorkingFamiliesResponseDetails
    {
        public const string StatusNotApplicable = "Not applicable";
        public const string StatusNotDueYet = "Not due yet";
        public const string StatusDueNow = "Due now";
        public const string StatusOverdue = "Overdue";
        public const string StatusChildTooOld = "Child too old";
        public static readonly string[] ReconfirmationStatusNotApplicable = {StatusNotApplicable,"grey"};
        public static readonly string[] ReconfirmationStatusNotDueYet = { StatusNotDueYet, "green"};
        public static readonly string[] ReconfirmationStatusDueNow = { StatusDueNow, "yellow"};
        public static readonly string[] ReconfirmationStatusOverdue = { StatusOverdue, "red"};
        public static readonly string[] ReconfirmationStatusChildTooOld = { StatusChildTooOld, "grey"};


    }
}