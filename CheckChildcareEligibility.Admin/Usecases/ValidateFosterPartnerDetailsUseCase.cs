using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public class FosterPartnerDetailsValidationResult
{
    public bool IsValid { get; set; }
    public Guid ConflictingFosterCarerId { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
}

public interface IValidateFosterPartnerDetailsUseCase
{
    Task<FosterPartnerDetailsValidationResult> Execute(FosterPartnerDetailsViewModel request, ModelStateDictionary modelState);
}

public class ValidateFosterPartnerDetailsUseCase : IValidateFosterPartnerDetailsUseCase
{
    private readonly ILogger<ValidateFosterPartnerDetailsUseCase> _logger;
    private readonly IFosterFamiliesGateway _gateway;

    public ValidateFosterPartnerDetailsUseCase(
        ILogger<ValidateFosterPartnerDetailsUseCase> logger,
        IFosterFamiliesGateway gateway)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    }

    public async Task<FosterPartnerDetailsValidationResult> Execute(FosterPartnerDetailsViewModel viewModel, ModelStateDictionary modelState)
    {
        var result = new FosterPartnerDetailsValidationResult();

        // If model passes form validation construct date fields then perform additional validation using FluentValidation
        if (modelState.IsValid)
        {
            // Set DateOfBirth in request before serializing
            viewModel.PartnerDateOfBirth = new DateTime(
                int.Parse(viewModel.Year),
                int.Parse(viewModel.Month),
                int.Parse(viewModel.Day));

            var request = viewModel.BuildRequest();
            var validator = new FosterPartnerRequestValidator();
            var validationResult = validator.Validate(request);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }
        }

        if (modelState.IsValid)
        {
            var matches = await _gateway.GetFosterFamiliesSearchRecords(1, 2, viewModel.PartnerNationalInsuranceNumber);
            var conflicts = matches.Data.Where(x => x.FosterCarerId != viewModel.FosterCarerId);

            if (conflicts.Any())
            {
                result.ConflictingFosterCarerId = conflicts.First().FosterCarerId;
                modelState.AddModelError("PartnerNationalInsuranceNumber", ValidationMessages.PartnerAlreadyExists);
            }
        }

        result.IsValid = modelState.IsValid;
        if (!modelState.IsValid)
        {
            result.Errors = ProcessModelStateErrors(modelState);
        }

        return result;
    }

    private Dictionary<string, List<string>> ProcessModelStateErrors(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(
                k => k.Key,
                v => v.Value.Errors.Select(e => e.ErrorMessage).ToList()
            );

        return errors;
    }
}