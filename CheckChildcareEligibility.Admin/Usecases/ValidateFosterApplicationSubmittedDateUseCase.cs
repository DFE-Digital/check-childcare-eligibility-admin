using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public class FosterApplicationSubmittedDateValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
}

public interface IValidateFosterApplicationSubmittedDateUseCase
{
    FosterApplicationSubmittedDateValidationResult Execute(FosterApplicationSubmittedDateViewModel request, ModelStateDictionary modelState);
}

public class ValidateFosterApplicationSubmittedDateUseCase : IValidateFosterApplicationSubmittedDateUseCase
{
    private readonly ILogger<ValidateFosterApplicationSubmittedDateUseCase> _logger;

    public ValidateFosterApplicationSubmittedDateUseCase(ILogger<ValidateFosterApplicationSubmittedDateUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public FosterApplicationSubmittedDateValidationResult Execute(FosterApplicationSubmittedDateViewModel request, ModelStateDictionary modelState)
    {
        if (!modelState.IsValid)
        {
            var errors = ProcessModelStateErrors(modelState);
            return new FosterApplicationSubmittedDateValidationResult { IsValid = false, Errors = errors };
        }
        return new FosterApplicationSubmittedDateValidationResult { IsValid = true };
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