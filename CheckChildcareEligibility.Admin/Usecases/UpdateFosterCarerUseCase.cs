using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using FluentValidation;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface IUpdateFosterCarerUseCase
    {
        Task Execute(Guid fosterCarerId, int localAuthorityId, UpdateFosterCarerRequest request);
    }

    public class UpdateFosterCarerUseCase : IUpdateFosterCarerUseCase
    {
        private readonly IFosterFamiliesGateway _fosterFamiliesGateway;
        public UpdateFosterCarerUseCase(IFosterFamiliesGateway fosterFamiliesGateway)
        {
            _fosterFamiliesGateway = fosterFamiliesGateway;
        }

        public async Task Execute(Guid fosterCarerId, int localAuthorityId, UpdateFosterCarerRequest request)
        {
            if (fosterCarerId == Guid.Empty) throw new ValidationException(FosterFamilyValidationMessages.FosterCarerId);

            ArgumentNullException.ThrowIfNull(request);


            if (request.FosterCarerRequest is not null)
            {
                var validationResult =
                    new FosterCarerRequestValidator()
                        .Validate(request.FosterCarerRequest);

                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }

            if (request.FosterPartnerRequest is not null)
            {
                var validationResult =
                    new FosterPartnerRequestValidator()
                        .Validate(request.FosterPartnerRequest);

                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }

            await _fosterFamiliesGateway.UpdateFosterCarer(fosterCarerId, localAuthorityId, request);
        }
    }
}