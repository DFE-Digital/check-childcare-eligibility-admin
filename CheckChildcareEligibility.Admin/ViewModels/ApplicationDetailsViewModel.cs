using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.ViewModels;

public class ApplicationDetailsViewModel
{
    public ParentGuardian parentDetails { get; set; }

    public Child[] children { get; set; }
}