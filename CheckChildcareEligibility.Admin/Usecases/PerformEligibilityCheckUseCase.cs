using System.Text;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface IPerformEligibilityCheckUseCase
{
    Task<CheckEligibilityResponse> Execute(
        ParentGuardian parentRequest,
        ISession session
    );
}

public class PerformEligibilityCheckUseCase : IPerformEligibilityCheckUseCase
{
    private readonly ICheckGateway _checkGateway;

    public PerformEligibilityCheckUseCase(ICheckGateway checkGateway)
    {
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
    }

    public async Task<CheckEligibilityResponse> Execute(
        ParentGuardian parentRequest,
        ISession session)
    {
        session.Set("ParentLastName", Encoding.UTF8.GetBytes(parentRequest.LastName ?? string.Empty));

        // Build DOB string
        var dobString = new DateOnly(
            int.Parse(parentRequest.Year),
            int.Parse(parentRequest.Month),
            int.Parse(parentRequest.Day)
        ).ToString("yyyy-MM-dd");

        session.Set("ParentDOB", Encoding.UTF8.GetBytes(dobString));

        session.Set("ParentNINO", Encoding.UTF8.GetBytes(parentRequest.NationalInsuranceNumber ?? ""));

        // Build ECS request
        var checkEligibilityRequest = new CheckEligibilityRequest_Fsm
        {
            Data = new CheckEligibilityRequestData_Fsm
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