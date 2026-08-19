using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public class FosterChildDetailsValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
}

public interface IValidateFosterChildDetailsUseCase
{
    FosterChildDetailsValidationResult Execute(FosterChildDetailsViewModel request, ModelStateDictionary modelState);
}

public class ValidateFosterChildDetailsUseCase : IValidateFosterChildDetailsUseCase
{
    private readonly ILogger<ValidateFosterChildDetailsUseCase> _logger;

    public ValidateFosterChildDetailsUseCase(ILogger<ValidateFosterChildDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public FosterChildDetailsValidationResult Execute(FosterChildDetailsViewModel request, ModelStateDictionary modelState)
    {
        if (!modelState.IsValid)
        {
            var errors = ProcessModelStateErrors(modelState);
            return new FosterChildDetailsValidationResult { IsValid = false, Errors = errors };
        }
        return new FosterChildDetailsValidationResult { IsValid = true };
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