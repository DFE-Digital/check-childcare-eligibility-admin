using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;

namespace CheckChildcareEligibility.Admin.Models;

[ExcludeFromCodeCoverage]

public class CheckRowBase { 
    public string Ni { get; set; }
    public string DOB { get; set; }
    public string? Reference { get; set; }
}
public class CheckRow : CheckRowBase
{
    public string LastName { get; set; }

}
[ExcludeFromCodeCoverage]
public class CheckRowWorkingFamilies : CheckRowBase {
    public string EligibilityCode { get; set; }
    

}

[ExcludeFromCodeCoverage]
public class CheckRowRowMap : ClassMap<CheckRow>
{
    public CheckRowRowMap()
    {
        Map(m => m.LastName).Index(0);
        Map(m => m.DOB).Index(1);
        Map(m => m.Ni).Index(2);
        Map(m => m.Reference).Index(3);
    }
}
[ExcludeFromCodeCoverage]
public class CheckRowRowMapWorkingFamilies : ClassMap<CheckRowWorkingFamilies>
{
    public CheckRowRowMapWorkingFamilies()
    {
        Map(m => m.EligibilityCode).Index(0);
        Map(m => m.Ni).Index(1);
        Map(m => m.DOB).Index(2);
        Map(m => m.Reference).Index(3);
    }
}