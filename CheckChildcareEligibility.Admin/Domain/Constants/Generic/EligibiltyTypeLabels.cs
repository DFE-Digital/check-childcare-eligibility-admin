namespace CheckChildcareEligibility.Admin.Domain.Constants.EligibilityTypeLabels;

public static class EligibilityTypeLabels
{

    public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
    {
        { "WF", "Childcare for working families" },
        { "2YO", "Early learning for 2-year-olds" },
        { "EYPP", "Early years pupil premium" },
    };
}
