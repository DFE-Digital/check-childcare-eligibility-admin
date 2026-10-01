using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public class FosterCodeReconfirmedDateValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
}

public interface IValidateFosterCodeReconfirmDateUseCase
{
    FosterCodeReconfirmedDateValidationResult Execute(FosterCodeReconfirmDateViewModel request, ModelStateDictionary modelState);
}

public class ValidateFosterCodeReconfirmDateUseCase : IValidateFosterCodeReconfirmDateUseCase
{
    private readonly ILogger<ValidateFosterCodeReconfirmDateUseCase> _logger;

    public ValidateFosterCodeReconfirmDateUseCase(ILogger<ValidateFosterCodeReconfirmDateUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public FosterCodeReconfirmedDateValidationResult Execute(FosterCodeReconfirmDateViewModel viewModel, ModelStateDictionary modelState)
    {
        // If model passes form validation construct date fields then perform additional validation using FluentValidation
        if (modelState.IsValid)
        {
            if (viewModel.IsTodaySelected == true)
            {
                viewModel.SubmissionDate = DateTime.Now.Date;
            }
            else
            {
                // Set DateOfBirth in request before serializing
                viewModel.SubmissionDate = new DateTime(
                int.Parse(viewModel.Year),
                int.Parse(viewModel.Month),
                int.Parse(viewModel.Day));
            }
        }

        if (!modelState.IsValid)
        {
            var errors = ProcessModelStateErrors(modelState);
            return new FosterCodeReconfirmedDateValidationResult { IsValid = false, Errors = errors };
        }
        return new FosterCodeReconfirmedDateValidationResult { IsValid = true };
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