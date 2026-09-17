
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterFamilyCreatedViewModel
    {

        public FosterChildResponse Response { get; init; }

        public Term ValidFromTerm => Response.TermValidity.Current.Name != TermName.None ? Response.TermValidity.Current : Response.TermValidity.Next;

    }
}
