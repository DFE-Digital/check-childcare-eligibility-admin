using CheckChildcareEligibility.Admin.Attributes;
using CheckChildcareEligibility.Admin.Domain.Constants;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface IValidateParentDetailsUseCase
{
    ValidationResult Execute(ParentGuardian request, ModelStateDictionary modelState, string? eligibilityType);
}

public class ValidationResult
{
    public bool IsValid { get; set; }

    public Dictionary<string, List<string>> Errors { get; set; }
}

public class ValidateParentDetailsUseCase : IValidateParentDetailsUseCase
{
    private readonly ILogger<ValidateParentDetailsUseCase> _logger;

    public ValidateParentDetailsUseCase(ILogger<ValidateParentDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValidationResult Execute(ParentGuardian request, ModelStateDictionary modelState, string? eligibilityType)
    {
        var isEypp = string.Equals(eligibilityType, "EYPP", StringComparison.OrdinalIgnoreCase);
        var errors = ProcessModelStateErrors(modelState, isEypp);

        if (isEypp)
        {  // validate identifiers seperately for eypp
            ValidateIdentifiers(request, errors);
        }

        return errors.Count == 0
            ? new ValidationResult { IsValid = true }
            : new ValidationResult { IsValid = false, Errors = errors };
    }

    private static Dictionary<string, List<string>> ProcessModelStateErrors(
        ModelStateDictionary modelState,
        bool isEypp)
    {
        var errors = modelState
            .Where(x => x.Value.Errors.Count > 0)
            .Where(x => !IsIdentifierError(x.Key, isEypp)) //exclude nass validation and nino validation here is EYPP
            .ToDictionary(
                k => k.Key,
                v => v.Value.Errors.Select(e => e.ErrorMessage).ToList(),
                StringComparer.OrdinalIgnoreCase
            );

        return errors;
    }

    private static bool IsIdentifierError(string key, bool isEypp)
    {
        return string.Equals(key, nameof(ParentGuardian.NationalAsylumSeekerServiceNumber),
                   StringComparison.OrdinalIgnoreCase)
               || isEypp && string.Equals(key, nameof(ParentGuardian.NationalInsuranceNumber),
                   StringComparison.OrdinalIgnoreCase);
    }

    private static void ValidateIdentifiers(
        ParentGuardian request,
        Dictionary<string, List<string>> errors)
    {
        var hasNino = !string.IsNullOrEmpty(request.NationalInsuranceNumber);
        var hasNass = !string.IsNullOrEmpty(request.NationalAsylumSeekerServiceNumber);

        if (!hasNino && !hasNass)
        {
            var missingPropertyName = request.IdentifierType switch
            {
                IdentifierTypeConstants.Nino => nameof(ParentGuardian.NationalInsuranceNumber),
                _ => nameof(ParentGuardian.NationalAsylumSeekerServiceNumber)
            };
            AddError(errors, missingPropertyName, ValidationMessages.NI_or_NASS);
            return;
        }

        var useNino = request.IdentifierType switch
        {
            IdentifierTypeConstants.Nino => true,
            IdentifierTypeConstants.Nass => false,
            _ => hasNino
        };
        var propertyName = useNino
            ? nameof(ParentGuardian.NationalInsuranceNumber)
            : nameof(ParentGuardian.NationalAsylumSeekerServiceNumber);
        var value = useNino
            ? request.NationalInsuranceNumber
            : request.NationalAsylumSeekerServiceNumber;
        System.ComponentModel.DataAnnotations.ValidationAttribute attribute =
            useNino ? new NinoAttribute() : new NassAttribute();
        var result = attribute.GetValidationResult(
            value,
            new System.ComponentModel.DataAnnotations.ValidationContext(request)
            {
                MemberName = propertyName
            });

        if (result != System.ComponentModel.DataAnnotations.ValidationResult.Success)
        {
            AddError(errors, propertyName, result!.ErrorMessage!);
        }
    }

    private static void AddError(Dictionary<string, List<string>> errors, string propertyName, string message)
    {
        errors[propertyName] = [message];
    }
}