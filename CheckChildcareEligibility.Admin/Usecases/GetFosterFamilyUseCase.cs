using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface IGetFosterFamilyUseCase
{
    Task<FosterFamilyResponse> Execute(Guid fosterCarerId, int localAuthorityId, bool includeChildren = false);
}

public class GetFosterFamilyUseCase : IGetFosterFamilyUseCase
{
    private readonly IFosterFamiliesGateway _fosterFamiliesGateway;

    public GetFosterFamilyUseCase(IFosterFamiliesGateway fosterFamiliesGateway)
    {
        _fosterFamiliesGateway = fosterFamiliesGateway;
    }

    public async Task<FosterFamilyResponse> Execute(Guid fosterCarerId, int localAuthorityId, bool includeChildren = false)
    {
        if (fosterCarerId == Guid.Empty) throw new ValidationException(FosterFamilyValidationMessages.FosterCarerId);

        var result = await _fosterFamiliesGateway.GetFosterFamily(fosterCarerId, localAuthorityId, includeChildren);
        return result;
    }
}