namespace CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeConstants;

public static class EligibilityTypeConstants
{

    public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
    {
        { "WF", "Childcare for working families" },
        { "2YO", "Early learning for 2-year-olds" },
        { "EYPP", "Early years pupil premium" },
    };

    public static readonly Dictionary<string, string> GuidanceLinks = new() {

        { "2YO", "/Check/_2yo_Guidance" },
        { "EYPP", "/Check/Eypp_Guidance" }
    };
   
}
