using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CheckChildcareEligibility.Admin.Attributes;

namespace CheckChildcareEligibility.Admin.Models;

public class ParentGuardian
{
    [Name]
    [Required(ErrorMessage = "Enter parent or guardian's last name in full")]
    public string? LastName { get; set; }

    [NotMapped]
    [Dob("date of birth", "parent or guardian", null, "Day", "Month", "Year")]
    public string? DateOfBirth { get; set; }

    public string? Day { get; set; }

    public string? Month { get; set; }

    public string? Year { get; set; }

    [Nino] [MaxLength(13)] public string? NationalInsuranceNumber { get; set; }
}