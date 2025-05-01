using System.Text;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface IPerformEyppEligibilityCheckUseCase
{
    Task<CheckEligibilityResponse> Execute(
        ParentGuardian parentRequest,
        ISession session
    );
}

public class PerformEyppEligibilityCheckUseCase : IPerformEyppEligibilityCheckUseCase
{
    private readonly ICheckGateway _checkGateway;

    public PerformEyppEligibilityCheckUseCase(ICheckGateway checkGateway)
    {
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
    }

    public async Task<CheckEligibilityResponse> Execute(
        ParentGuardian parentRequest,
        ISession session)
    {
        session.Set("ParentFirstName", Encoding.UTF8.GetBytes(parentRequest.FirstName ?? string.Empty));
        session.Set("ParentLastName", Encoding.UTF8.GetBytes(parentRequest.LastName ?? string.Empty));

        // Build DOB string
        var dobString = new DateOnly(
            int.Parse(parentRequest.Year),
            int.Parse(parentRequest.Month),
            int.Parse(parentRequest.Day)
        ).ToString("yyyy-MM-dd");

        session.Set("ParentDOB", Encoding.UTF8.GetBytes(dobString));
        session.SetString("ParentEmail", parentRequest.EmailAddress);

        // If we're finishing a NASS flow, store "ParentNASS"; 
        // otherwise store "ParentNINO".
        if (parentRequest.NinAsrSelection == ParentGuardian.NinAsrSelect.AsrnSelected)
        {
            session.Set("ParentNASS", Encoding.UTF8.GetBytes(parentRequest.NationalAsylumSeekerServiceNumber ?? ""));
            session.Remove("ParentNINO");
        }
        else
        {
            session.Set("ParentNINO", Encoding.UTF8.GetBytes(parentRequest.NationalInsuranceNumber ?? ""));
            session.Remove("ParentNASS");
        }

        // Build ECS request
        var checkEligibilityRequest = new CheckEligibilityRequest
        {
            Data = new CheckEligibilityRequestData(Domain.Enums.CheckEligibilityType.EarlyYearPupilPremium)
            {
                LastName = parentRequest.LastName,
                NationalInsuranceNumber = parentRequest.NationalInsuranceNumber?.ToUpper(),
                DateOfBirth = dobString
            }
        };

        // Call ECS check


        var response = await _checkGateway.PostCheck(checkEligibilityRequest);

        return response;
    }
}