using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface IPreviewFosterCodeReconfirmUseCase
    {
        Task<FosterCodePreviewResponse> Execute(Guid fosterChildId, FosterChildReconfirmRequest request);
    }

    public class PreviewFosterCodeReconfirmUseCase : IPreviewFosterCodeReconfirmUseCase
    {
        private readonly IFosterFamiliesGateway _fosterFamiliesGateway;
        private readonly ILogger<PreviewFosterCodeReconfirmUseCase> _logger;

        public PreviewFosterCodeReconfirmUseCase(
            ILogger<PreviewFosterCodeReconfirmUseCase> logger,
            IFosterFamiliesGateway fosterFamiliesGateway)
        {
            _logger = logger;
            _fosterFamiliesGateway = fosterFamiliesGateway;
        }

        public async Task<FosterCodePreviewResponse> Execute(Guid fosterChildId, FosterChildReconfirmRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validator = new FosterChildReconfirmRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            return await _fosterFamiliesGateway.PreviewFosterChildReconfirm(fosterChildId, request);
        }
    }
}