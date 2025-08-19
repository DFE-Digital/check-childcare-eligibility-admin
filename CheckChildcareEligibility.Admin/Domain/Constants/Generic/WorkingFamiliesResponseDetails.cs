namespace CheckChildcareEligibility.Admin.Domain.Constants.Generic
{
    public static class WorkingFamiliesResponseDetails
    {
        public const string StatusNotApplicable = "Not applicable";
        public const string StatusNotDueYet = "Not due yet";
        public static readonly string[] ReconfirmationStatusNotApplicable = {StatusNotApplicable,"yellow" };
        public static readonly string[] ReconfirmationStatusNotDueYet = {StatusNotDueYet,"green" };


    }
}