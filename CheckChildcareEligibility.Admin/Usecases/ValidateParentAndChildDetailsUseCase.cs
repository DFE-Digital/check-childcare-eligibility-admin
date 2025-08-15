using CheckChildcareEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public class CombinedValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
}

public interface IValidateParentAndChildDetailsUseCase
{
    CombinedValidationResult Execute(ParentAndChildViewModel request, ModelStateDictionary modelState);
}

public class ValidateParentAndChildDetailsUseCase : IValidateParentAndChildDetailsUseCase
{
    private readonly ILogger<ValidateParentAndChildDetailsUseCase> _logger;

    public ValidateParentAndChildDetailsUseCase(ILogger<ValidateParentAndChildDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public CombinedValidationResult Execute(ParentAndChildViewModel request, ModelStateDictionary modelState)
    {
        if (!modelState.IsValid)
        {
            var errors = ProcessModelStateErrors(modelState);
            return new CombinedValidationResult { IsValid = false, Errors = errors };
        }
        return new CombinedValidationResult { IsValid = true };
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