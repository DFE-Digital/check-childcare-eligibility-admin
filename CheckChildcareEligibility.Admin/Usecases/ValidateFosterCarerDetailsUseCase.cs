using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public class FosterCarerDetailsValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
}

public interface IValidateFosterCarerDetailsUseCase
{
    FosterCarerDetailsValidationResult Execute(FosterCarerDetailsViewModel request, ModelStateDictionary modelState);
}

public class ValidateFosterCarerDetailsUseCase : IValidateFosterCarerDetailsUseCase
{
    private readonly ILogger<ValidateFosterCarerDetailsUseCase> _logger;

    public ValidateFosterCarerDetailsUseCase(ILogger<ValidateFosterCarerDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public FosterCarerDetailsValidationResult Execute(FosterCarerDetailsViewModel viewModel, ModelStateDictionary modelState)
    {
        // If model passes form validation construct date fields then perform additional validation using FluentValidation
        if (modelState.IsValid)
        {
            viewModel.CarerDateOfBirth = new DateTime(
                int.Parse(viewModel.Year),
                int.Parse(viewModel.Month),
                int.Parse(viewModel.Day));

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
        if (!modelState.IsValid)
        {
            var errors = ProcessModelStateErrors(modelState);
            return new FosterCarerDetailsValidationResult { IsValid = false, Errors = errors };
        }
        return new FosterCarerDetailsValidationResult { IsValid = true };
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