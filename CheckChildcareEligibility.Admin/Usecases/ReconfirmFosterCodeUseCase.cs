using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface IReconfirmFosterCodeUseCase
    {
        Task<FosterChildResponse> Execute(Guid fosterChildId, FosterChildReconfirmRequest request);
    }

    public class ReconfirmFosterCodeUseCase : IReconfirmFosterCodeUseCase
    {
        private readonly IFosterFamiliesGateway _fosterFamiliesGateway;
        private readonly ILogger<ReconfirmFosterCodeUseCase> _logger;
        private readonly bool _enforceSubmissionDateWindow;

        public ReconfirmFosterCodeUseCase(
            ILogger<ReconfirmFosterCodeUseCase> logger,
            IFosterFamiliesGateway fosterFamiliesGateway,
            IConfiguration configuration)
        {
            _enforceSubmissionDateWindow = configuration?.GetValue("FosterFamilies:EnforceSubmissionDateWindow", true) ?? true;
            _logger = logger;
            _fosterFamiliesGateway = fosterFamiliesGateway;
        }

        public async Task<FosterChildResponse> Execute(Guid fosterChildId, FosterChildReconfirmRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validator = new FosterChildReconfirmRequestValidator(_enforceSubmissionDateWindow);
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            return await _fosterFamiliesGateway.ReconfirmFosterChild(fosterChildId, request);
        }
    }
}