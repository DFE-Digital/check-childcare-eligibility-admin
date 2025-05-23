using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;

namespace CheckChildcareEligibility.Admin.Models;

[ExcludeFromCodeCoverage]
public class CheckRow
{
    public string LastName { get; set; }
    public string DOB { get; set; }
    public string Ni { get; set; }
    public string Nass { get; set; }
}

[ExcludeFromCodeCoverage]
public class CheckRowRowMap : ClassMap<CheckRow>
{
    public CheckRowRowMap()
    {
        Map(m => m.LastName).Index(0);
        Map(m => m.DOB).Index(1);
        Map(m => m.Ni).Index(2);
        Map(m => m.Nass).Index(3);
    }
}