using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface IPreviewFosterFamilyCodeUseCase
    {
        Task<FosterCodePreviewResponse> Execute(FosterFamilyRequest request);
    }

    public class PreviewFosterFamilyCodeUseCase : IPreviewFosterFamilyCodeUseCase
    {
        private readonly IFosterFamiliesGateway _fosterFamiliesGateway;
        private readonly ILogger<PreviewFosterFamilyCodeUseCase> _logger;

        public PreviewFosterFamilyCodeUseCase(
            ILogger<PreviewFosterFamilyCodeUseCase> logger,
            IFosterFamiliesGateway fosterFamiliesGateway)
        {
            _logger = logger;
            _fosterFamiliesGateway = fosterFamiliesGateway;
        }

        public async Task<FosterCodePreviewResponse> Execute(FosterFamilyRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validator = new FosterFamilyRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            return await _fosterFamiliesGateway.PreviewFosterFamilyCode(request);
        }
    }
}