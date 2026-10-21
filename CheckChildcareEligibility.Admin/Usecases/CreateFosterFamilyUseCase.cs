using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface ICreateFosterFamilyUseCase
    {
        Task<FosterFamilyCreatedResponse> Execute(FosterFamilyRequest request);
    }

    public class CreateFosterFamilyUseCase : ICreateFosterFamilyUseCase
    {
        private readonly IFosterFamiliesGateway _fosterFamiliesGateway;
        private readonly ILogger<CreateFosterFamilyUseCase> _logger;
        private readonly bool _enforceSubmissionDateWindow;

        public CreateFosterFamilyUseCase(
            ILogger<CreateFosterFamilyUseCase> logger,
            IFosterFamiliesGateway fosterFamiliesGateway,
            IConfiguration? configuration = null)
        {
            _enforceSubmissionDateWindow = configuration?.GetValue("FosterFamilies:EnforceSubmissionDateWindow", true) ?? true;
            _logger = logger;
            _fosterFamiliesGateway = fosterFamiliesGateway;
        }

        public async Task<FosterFamilyCreatedResponse> Execute(FosterFamilyRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validator = new FosterFamilyRequestValidator(_enforceSubmissionDateWindow);
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            request.FosterCarer.CarerNationalInsuranceNumber =
                NinoValidation.Normalize(
                    request.FosterCarer.CarerNationalInsuranceNumber);

            if (request.HasPartner && request.Partner is not null)
            {
                request.Partner.PartnerNationalInsuranceNumber =
                    NinoValidation.Normalize(
                        request.Partner.PartnerNationalInsuranceNumber);
            }

            return await _fosterFamiliesGateway.CreateFosterFamily(request);
        }
    }
}