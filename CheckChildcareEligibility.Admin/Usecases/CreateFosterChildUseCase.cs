using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using FluentValidation;

namespace CheckChildcareEligibility.Admin.Usecases;

public interface ICreateFosterChildUseCase
{
    Task<FosterChildResponse> Execute(Guid fosterCarerId, FosterChildRequest request, DateTime submissionDate);
}

public class CreateFosterChildUseCase : ICreateFosterChildUseCase
{
    private readonly IFosterFamiliesGateway _fosterFamiliesGateway;

    public CreateFosterChildUseCase(IFosterFamiliesGateway fosterFamiliesGateway)
    {
        _fosterFamiliesGateway = fosterFamiliesGateway;
    }

    public async Task<FosterChildResponse> Execute(Guid fosterCarerId, FosterChildRequest request, DateTime submissionDate)
    {
        if (fosterCarerId == Guid.Empty)
        {
            throw new ValidationException(FosterFamilyValidationMessages.FosterCarerId);
        }

        ArgumentNullException.ThrowIfNull(request);

        var validationResult = new FosterChildRequestValidator().Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        request.SubmissionDate = submissionDate;
        return await _fosterFamiliesGateway.CreateFosterChild(fosterCarerId, request);
    }
}
