using System.ComponentModel.DataAnnotations.Schema;
using CheckChildcareEligibility.Admin.Attributes;

namespace CheckChildcareEligibility.Admin.Models;

public class Child
{
    public string? EligibilityCode { get; set; }

    [NotMapped]
    [Dob("date of birth", "child", "ChildIndex", "Day", "Month", "Year", true, true)]
    public string? ChildDateOfBirth { get; set; }

    public string? Day { get; set; }

    public string? Month { get; set; }

    public string? Year { get; set; }
}