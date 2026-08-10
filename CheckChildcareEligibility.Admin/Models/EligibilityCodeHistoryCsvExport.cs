namespace CheckChildcareEligibility.Admin.Models
{
    using CsvHelper.Configuration.Attributes;

public class EligibilityCodeHistoryCsvExport
{
    [Name("Event")]
    public string Event { get; set; }

    [Name("Submitted on")]
    public string? SubmissionDate { get; set; }

    [Name("Discretionary start date")]
    public string? DiscretionaryStartDate { get; set; }

    [Name("Validity start date")]
    public string? ValidityStartDate { get; set; }

    [Name("Validity end date")]
    public string? ValidityEndDate { get; set; }

    [Name("Grace period ends")]
    public string? GracePeriodEndDate { get; set; }

    [Name("Event ID")]
    public string EventId { get; set; }

    [Name("Parent National Insurance number")]
    public string ParentNationalInsuranceNumber { get; set; }

    [Name("Parent first name")]
    public string ParentFirstName { get; set; }

    [Name("Parent last name")]
    public string ParentLastName { get; set; }

    [Name("Parent date of birth")]
    public string? ParentDateOfBirth { get; set; }

    [Name("Partner National Insurance number")]
    public string PartnerNationalInsuranceNumber { get; set; }

    [Name("Partner first name")]
    public string PartnerFirstName { get; set; }

    [Name("Partner last name")]
    public string PartnerLastName { get; set; }

    [Name("Partner date of birth")]
    public string? PartnerDateOfBirth { get; set; }

    [Name("Child first name")]
    public string ChildFirstName { get; set; }

    [Name("Child last name")]
    public string ChildLastName { get; set; }

    [Name("Child date of birth")]
    public string? ChildDateOfBirth { get; set; }

    [Name("Child postcode")]
    public string ChildPostCode { get; set; }
}
}
