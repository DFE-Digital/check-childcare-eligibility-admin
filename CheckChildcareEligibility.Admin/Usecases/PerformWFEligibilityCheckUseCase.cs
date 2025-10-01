using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.ViewModels;
using System.Text;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface IPerformWFEligibilityCheckUseCase
{
    Task<CheckEligibilityResponse> Execute(
        ParentAndChildViewModel parentAndChildRequest,
        ISession session
    );
    Task<CheckEligibilityItemWorkingFamiliesResponse> GetItemAsync(string getEligibilityCheck);
}

public class PerformWFEligibilityCheckUseCase : IPerformWFEligibilityCheckUseCase
{
    private readonly ICheckGateway _checkGateway;

    public PerformWFEligibilityCheckUseCase(ICheckGateway checkGateway)
    {
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
    }

    public async Task<CheckEligibilityResponse> Execute(
        ParentAndChildViewModel parentAndChildRequest,
        ISession session)
    {
        session.Set("EligibilityCode", Encoding.UTF8.GetBytes(parentAndChildRequest.Child.EligibilityCode ?? string.Empty));

        // Build DOB string
        var dobString = new DateOnly(
            int.Parse(parentAndChildRequest.Child.Year),
            int.Parse(parentAndChildRequest.Child.Month),
            int.Parse(parentAndChildRequest.Child.Day)
        ).ToString("yyyy-MM-dd");

        session.Set("ChildDOB", Encoding.UTF8.GetBytes(dobString));

        session.Set("ParentNINO", Encoding.UTF8.GetBytes(parentAndChildRequest.NationalInsuranceNumber ?? ""));

        // Build ECS request
        var checkEligibilityRequest = new CheckEligibilityRequest
        {
            Data = new CheckEligibilityRequestData(Domain.Enums.CheckEligibilityType.WorkingFamilies)
            {
                EligibilityCode = parentAndChildRequest.Child.EligibilityCode,
                NationalInsuranceNumber = parentAndChildRequest.NationalInsuranceNumber?.ToUpper(),
                DateOfBirth = dobString
            }
        };

        // Call ECS check
        var response = await _checkGateway.PostCheck(checkEligibilityRequest);

        return response;
    }
    public async Task<CheckEligibilityItemWorkingFamiliesResponse> GetItemAsync(string getEligibilityCheck) {
        var response = await _checkGateway.GetWFResult(getEligibilityCheck);
        return response;

    }
}