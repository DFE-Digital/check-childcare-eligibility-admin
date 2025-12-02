using CsvHelper.Configuration.Attributes;

namespace CheckChildcareEligibility.Admin.Models;

public interface IBulkExport { }
public class BulkExport : IBulkExport
{
    [Name("Parent NI Number")] public string NI { get; set; }

    [Name("Parent Date of Birth")] public string DOB { get; set; }

    [Name("Parent Last Name")] public string LastName { get; set; }

    [Name("Outcome")] public string Outcome { get; set; }
}

public class BulkExportWorkingFamilies : IBulkExport
{
    [Name("Eligibility code")] public string EligibilityCode { get; set; }

    [Name("National Insurance number")] public string NI { get; set; }

    [Name("Child date of birth")] public string ChildDOB { get; set; }

    [Name("Outcome")] public string Outcome { get; set; }

    // 28-11-2025 This will be the returned VSD from the API for now 
    [Name("Eligibility confirmed on")] public string EligibilityConfirmedOn { get; set; }
    // 28-11-2025 This will be the returned VED from the API 
    [Name("Reconfirm by")] public string ReconfirmBy { get; set; }
    [Name("Grace period ends")] public string GracePeriodEnds { get; set; }




}