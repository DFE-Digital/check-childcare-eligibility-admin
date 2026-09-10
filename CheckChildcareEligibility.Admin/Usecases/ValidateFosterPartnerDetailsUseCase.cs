using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public class FosterPartnerDetailsValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
}

public interface IValidateFosterPartnerDetailsUseCase
{
    FosterPartnerDetailsValidationResult Execute(FosterPartnerDetailsViewModel request, ModelStateDictionary modelState);
}

public class ValidateFosterPartnerDetailsUseCase : IValidateFosterPartnerDetailsUseCase
{
    private readonly ILogger<ValidateFosterPartnerDetailsUseCase> _logger;

    public ValidateFosterPartnerDetailsUseCase(ILogger<ValidateFosterPartnerDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public FosterPartnerDetailsValidationResult Execute(FosterPartnerDetailsViewModel viewModel, ModelStateDictionary modelState)
    {
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

        if (!modelState.IsValid)
        {
            var errors = ProcessModelStateErrors(modelState);
            return new FosterPartnerDetailsValidationResult { IsValid = false, Errors = errors };
        }
        return new FosterPartnerDetailsValidationResult { IsValid = true };
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