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

        public CreateFosterFamilyUseCase(
            ILogger<CreateFosterFamilyUseCase> logger,
            IFosterFamiliesGateway fosterFamiliesGateway)
        {
            _logger = logger;
            _fosterFamiliesGateway = fosterFamiliesGateway;
        }

        public async Task<FosterFamilyCreatedResponse> Execute(FosterFamilyRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validator = new FosterFamilyRequestValidator();
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