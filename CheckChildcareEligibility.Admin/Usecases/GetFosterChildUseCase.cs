using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface IGetFosterChildUseCase
    {
        Task<FosterChildResponse> Execute(Guid fosterChildId, int localAuthorityId, bool includeFosterCarer = false);
    }

    public class GetFosterChildUseCase : IGetFosterChildUseCase
    {
        private readonly IFosterFamiliesGateway _fosterFamiliesGateway;

        public GetFosterChildUseCase(IFosterFamiliesGateway fosterFamiliesGateway)
        {
            _fosterFamiliesGateway = fosterFamiliesGateway;
        }

        public async Task<FosterChildResponse> Execute(Guid fosterChildId, int localAuthorityId, bool includeFosterCarer = false)
        {
            if (fosterChildId == Guid.Empty) throw new ValidationException(FosterFamilyValidationMessages.FosterChildId);

            var result = await _fosterFamiliesGateway.GetFosterChild(fosterChildId, localAuthorityId, includeFosterCarer);
            if (result == null) throw new KeyNotFoundException($"Foster child {fosterChildId} not found");

            return result;
        }
    }
}
