namespace CheckChildcareEligibility.Admin.Domain.Constants.Generic;

public static class BulkCheckFieldDescriptionConstants
{
    public const string EligibilityCode = "the eligibility code";

    public const string WorkingFamiliesNationalInsuranceNumber =
        "a National Insurance number (can be for a parent, guardian or partner)";

    public const string ChildDateOfBirth =
        "the child's date of birth (format DD/MM/YYYY or YYYY-MM-DD)";

    public const string ParentLastName = "last name";

    public const string ParentDateOfBirth = "date of birth (format DD/MM/YYYY or YYYY-MM-DD)";

    public const string ParentNationalInsuranceNumber = "National Insurance number";

    public const string NationalInsuranceOrAsylumSupportReferenceNumber =
        "National insurance number or asylum support reference number";
}