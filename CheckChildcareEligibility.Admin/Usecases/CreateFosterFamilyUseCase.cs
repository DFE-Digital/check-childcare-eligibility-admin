using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface ICreateFosterFamilyUseCase
    {
        Task<FosterFamilyCreatedResponse> Execute(FosterFamilyRequest request, int localAuthorityId);
    }

    public class CreateFosterFamilyUseCase : ICreateFosterFamilyUseCase
    {
        private readonly ICheckGateway _checkGateway;
        private readonly ILogger<CreateFosterFamilyUseCase> _logger;

        public CreateFosterFamilyUseCase(
            ILogger<CreateFosterFamilyUseCase> logger,
            ICheckGateway checkGateway)
        {
            _logger = logger;
            _checkGateway = checkGateway;
        }

        public async Task<FosterFamilyCreatedResponse> Execute(FosterFamilyRequest request, int localAuthorityId)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validator = new FosterFamilyRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(
                    validationResult.Errors);
            }

            request.FosterCarer.LocalAuthorityID = localAuthorityId;

            return await _checkGateway.CreateFosterFamily(request);
        }
    }
}