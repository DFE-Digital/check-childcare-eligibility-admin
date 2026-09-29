using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using FluentValidation;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface IUpdateFosterChildUseCase
    {
        Task Execute(Guid fosterChildId, int localAuthorityId, UpdateFosterChildRequest request);
    }

    public class UpdateFosterChildUseCase : IUpdateFosterChildUseCase
    {
        private readonly IFosterFamiliesGateway _fosterFamiliesGateway;
        public UpdateFosterChildUseCase(IFosterFamiliesGateway fosterFamiliesGateway)
        {
            _fosterFamiliesGateway = fosterFamiliesGateway;
        }

        public async Task Execute(Guid fosterChildId, int localAuthorityId, UpdateFosterChildRequest request)
        {
            if (fosterChildId == Guid.Empty) throw new ValidationException(FosterFamilyValidationMessages.FosterChildId);
            ArgumentNullException.ThrowIfNull(request);

            if (request.FosterChildRequest is not null)
            {
                var validationResult = new FosterChildRequestValidator().Validate(request.FosterChildRequest);

                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }

            await _fosterFamiliesGateway.UpdateFosterChild(fosterChildId, localAuthorityId, request);
        }
    }
}