using System.ComponentModel.DataAnnotations.Schema;
using CheckChildcareEligibility.Admin.Attributes;

namespace CheckChildcareEligibility.Admin.Models;

public class Child
{
    //[NotMapped] public int ChildIndex { get; set; }

    //[ChildName("first name")] public string? FirstName { get; set; }

    //[ChildName("last name")]
    //[LastName("last name", "child", "ChildIndex")]
    //public string? LastName { get; set; }

    public string? EligibilityCode { get; set; }

    [NotMapped]
    [Dob("date of birth", "child", "ChildIndex", "Day", "Month", "Year", true, true)]
    public string? ChildDateOfBirth { get; set; }

    public string? Day { get; set; }

    public string? Month { get; set; }

    public string? Year { get; set; }

    //public School? School { get; set; }
}