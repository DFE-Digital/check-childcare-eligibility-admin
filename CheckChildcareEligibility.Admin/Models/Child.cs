using CheckChildcareEligibility.Admin.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckChildcareEligibility.Admin.Models;

public class Child
{
    [EligibilityCode]
    [MaxLength(11)]
    public string? EligibilityCode { get; set; }

    [NotMapped]
    [Dob("date of birth", "child", "ChildIndex", "Day", "Month", "Year", true, true)]
    public string? ChildDateOfBirth { get; set; }

    public string? Day { get; set; }

    public string? Month { get; set; }

    public string? Year { get; set; }
}