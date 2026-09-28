using CheckChildcareEligibility.Admin.Attributes;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterCarerConflictViewModel
    {

        public FosterCarerDetailsViewModel CarerDetails { get; set; }

        public FosterFamilyResponse ConflictingFamily {get;set;}

    }
}