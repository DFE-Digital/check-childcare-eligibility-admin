using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public class FosterCarerDetailsValidationResult
{
    public bool IsValid { get; set; }

    public Guid ConflictingFosterCarerId { get; set; }

    public Dictionary<string, List<string>> Errors { get; set; }
}

public interface IValidateFosterCarerDetailsUseCase
{
    Task<FosterCarerDetailsValidationResult> Execute(FosterCarerDetailsViewModel request, ModelStateDictionary modelState);
}

public class ValidateFosterCarerDetailsUseCase : IValidateFosterCarerDetailsUseCase
{
    private readonly ILogger<ValidateFosterCarerDetailsUseCase> _logger;

    private readonly IFosterFamiliesGateway _gateway;

    public ValidateFosterCarerDetailsUseCase(
        ILogger<ValidateFosterCarerDetailsUseCase> logger,
        IFosterFamiliesGateway gateway
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gateway = gateway;
    }

    public async Task<FosterCarerDetailsValidationResult> Execute(FosterCarerDetailsViewModel viewModel, ModelStateDictionary modelState)
    {
        FosterCarerDetailsValidationResult result = new();

        // If model passes form validation construct date fields then perform additional validation using FluentValidation
        if (modelState.IsValid)
        {
            // Set DateOfBirth in request before serializing
            viewModel.CarerDateOfBirth = new DateTime(
                int.Parse(viewModel.Year),
                int.Parse(viewModel.Month),
                int.Parse(viewModel.Day));
            
            // Remove spaces from NINO before validation
            viewModel.CarerNationalInsuranceNumber = NinoValidation.RemoveSpaces(viewModel.CarerNationalInsuranceNumber);

            var request = viewModel.BuildRequest();
            var validator = new FosterCarerRequestValidator();
            var validationResult = validator.Validate(request);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }
        }

        // If model is still valid, perform extra validation to detect existing foster carers/partners with the same NINOs
        if (modelState.IsValid)
        {
            // Find matching foster carer records by NINO
            var matches = await _gateway.GetFosterFamiliesSearchRecords(1, 2, viewModel.CarerNationalInsuranceNumber);

            // Ignore the current FosterCarer record if this is an edit operation
            var conflicts = matches.Data.Where(x => x.FosterCarerId != viewModel.FosterCarerId);

            // If conflict is detected then populate the conflicting foster carer Id and return failure
            if (conflicts.Any())
            {
                result.ConflictingFosterCarerId = conflicts.FirstOrDefault().FosterCarerId;
                modelState.AddModelError("CarerNationalInsuranceNumber", ValidationMessages.CarerAlreadyExists);
            }
        }
        
        // Set result state and populate errors in result of failure
        result.IsValid = modelState.IsValid;
        if (!modelState.IsValid) { result.Errors = ProcessModelStateErrors(modelState); }
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