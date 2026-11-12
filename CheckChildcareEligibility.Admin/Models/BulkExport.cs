using CsvHelper.Configuration.Attributes;

namespace CheckChildcareEligibility.Admin.Models;

public interface IBulkExport
{ }

public class BulkExportBase : IBulkExport
{
    [Index(1), Name("Parent Date of Birth")]
    public string DOB { get; set; }

    [Index(0), Name("Parent Last Name")]
    public string LastName { get; set; }

    [Index(2), Name("Parent National Insurance number")]
    public string NI { get; set; }

    public class BulkExport2YO : BulkExportBase
    {
        [Index(3), Name("Outcome")]
        public string Outcome { get; set; }

        [Index(4), Name("Reference (optional)")]
        public string? Reference { get; set; }
    }

    public class BulkExportEYPP : BulkExportBase
    {
        [Index(3), Name("Parent Asylum Seeker Reference number")]
        public string NASS { get; set; }

        [Index(4), Name("Outcome")]
        public string Outcome { get; set; }

        [Index(5), Name("Reference (optional)")]
        public string? Reference { get; set; }
    }
}

public class BulkExportWorkingFamilies : IBulkExport
{
    [Index(2), Name("Child date of birth")] public string ChildDOB { get; set; }

    [Index(0), Name("Eligibility code")] public string EligibilityCode { get; set; }

    [Index(6), Name("Grace period ends")] public string GracePeriodEnds { get; set; }

    [Index(1), Name("National Insurance number")] public string NI { get; set; }

    [Index(3), Name("Outcome")] public string Outcome { get; set; }

    [Index(7), Name("Reference (optional)")]
    public string? Reference { get; set; }

    // 28-11-2025 This will be the returned VED from the API
    [Index(5), Name("Validity end date")] public string ValidityEndDate { get; set; }

    // 28-11-2025 This will be the returned VSD from the API for now
    [Index(4), Name("Validity start date")] public string ValidityStartDate { get; set; }
}