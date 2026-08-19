using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Models;
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

    public FosterPartnerDetailsValidationResult Execute(FosterPartnerDetailsViewModel request, ModelStateDictionary modelState)
    {
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