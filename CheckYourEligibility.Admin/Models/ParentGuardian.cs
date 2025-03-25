﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CheckYourEligibility.Admin.Attributes;

namespace CheckYourEligibility.Admin.Models;

public class ParentGuardian
{
    public enum NinAsrSelect
    {
        None,
        NinSelected,
        AsrnSelected
    }

    [Name]
    [Required(ErrorMessage = "Enter a first name")]
    public string? FirstName { get; set; }

    [Name]
    [Required(ErrorMessage = "Enter a last name")]
    public string? LastName { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "Enter an email")]
    public string? EmailAddress { get; set; }

    [NotMapped]
    [Dob("date of birth", "parent or guardian", null, "Day", "Month", "Year", true, false)]
    public string? DateOfBirth { get; set; }

    public string? Day { get; set; }

    public string? Month { get; set; }

    public string? Year { get; set; }

    [Nino] [MaxLength(13)] public string? NationalInsuranceNumber { get; set; }

    [Nass] [MaxLength(10)] public string? NationalAsylumSeekerServiceNumber { get; set; }

    [NotMapped] public bool? NINAS { get; set; }

    public NinAsrSelect NinAsrSelection { get; set; }
}