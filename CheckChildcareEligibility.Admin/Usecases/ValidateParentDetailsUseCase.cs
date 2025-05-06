using CheckChildcareEligibility.Admin.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
}

public interface IValidateParentDetailsUseCase
{
    ValidationResult Execute(ParentGuardian request, ModelStateDictionary modelState);
}

public class ValidateParentDetailsUseCase : IValidateParentDetailsUseCase
{
    private readonly ILogger<ValidateParentDetailsUseCase> _logger;

    public ValidateParentDetailsUseCase(ILogger<ValidateParentDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValidationResult Execute(ParentGuardian request, ModelStateDictionary modelState)
    {

        if (!modelState.IsValid)
        {
            var errors = ProcessModelStateErrors(modelState);
            return new ValidationResult { IsValid = false, Errors = errors };
        }
        return new ValidationResult { IsValid = true };
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