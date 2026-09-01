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

    public FosterCarerDetailsValidationResult Execute(FosterCarerDetailsViewModel request, ModelStateDictionary modelState)
    {
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